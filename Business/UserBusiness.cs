using Common.DTO.GeoVictoria;
using Common.DTO.Rex;
using Common.Entity;
using Common.Enum;
using Common.Helper;
using Common.ViewModels;
using Common.ViewModels.API;
using DAO.GeoVictoria;
using DAO.Rex;
using Helper;
using Helpers;
using IBusiness;
using IDAO.GeoVictoria;
using IDAO.Rex;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Business
{
    public class UserBusiness : IUserBusiness
    {
        private readonly IGroupBusiness GroupBusiness;        
        private readonly IPositionBusiness PositionBusiness;
        private readonly IUserGeoVictoriaDAO UserGeoVictoriaDAO;
        private readonly IPersonaRexDAO PersonaRexDAO;
        private readonly ICatalogoRexDAO CatalogoRexDAO;
        private readonly IDesarrolloOrganizacionalRexDAO DesarrolloOrganizacionalRexDAO;
        private readonly VersionConfiguration versionConfiguration;
        private readonly IContractGeoVictoriaDAO ContractGeoVictoriaDAO;
        private readonly IClienteExternoDAO ClienteExternoDAO;

        private readonly int MaxParallel = int.TryParse(ConfigurationHelper.Value("maxParallelUsers"), out int maxProcess) ? maxProcess : 15;
        private readonly DateTime nullDateTime = new DateTime(1, 1, 1, 0, 0, 0);

        public UserBusiness(RexExecutionVM rexExecution)
        {
            this.GroupBusiness = new GroupBusiness();
            this.PositionBusiness = new PositionBusiness();
            this.UserGeoVictoriaDAO = new UserGeoVictoriaDAO();
            this.PersonaRexDAO = new PersonaRexDAO();
            this.CatalogoRexDAO = new CatalogoRexDAO();
            this.DesarrolloOrganizacionalRexDAO = new DesarrolloOrganizacionalRexDAO();
            this.ContractGeoVictoriaDAO = new ContractGeoVictoriaDAO();
            this.ClienteExternoDAO = new ClienteExternoDAO();
            this.versionConfiguration = new VersionConfiguration(rexExecution.RexVersion);
        }

        public void SynchronizeUsers(RexExecutionVM rexExecutionVM)
        {
            ProcessUserDataObjectVM data = this.GetData(rexExecutionVM);

            if (data.RexUsers != null && data.RexUsers.Count > 0)
            {
                List<TransactionUserVM> usersToProcess = this.GetUsersToProcess(rexExecutionVM, data);

                this.ProcessUsers(rexExecutionVM, usersToProcess, new GeoVictoriaConnectionVM()
                {
                    TestEnvironment = rexExecutionVM.TestEnvironment,
                    ApiKey = rexExecutionVM.ApiKey,
                    ApiSecret = rexExecutionVM.ApiSecret
                });

                if (rexExecutionVM.UseMultiContracts)
                {
                    data.Quotes = this.GetQuote(rexExecutionVM);
                    this.SendContractsToApi(data.GeoVictoriaUsers, data.Contracts, rexExecutionVM, data.Quotes);
                }
            }
        }

        private ProcessUserDataObjectVM GetData(RexExecutionVM rexExecutionVM)
        {
            ProcessUserDataObjectVM data = new ProcessUserDataObjectVM();
            ICompanyBusiness companyBusiness = new CompanyBusiness();
            data.RexCompanies = companyBusiness.GetRexCompanies(rexExecutionVM, versionConfiguration);
            data.GeoVictoriaUsers = this.UserGeoVictoriaDAO.GetAll(new GeoVictoriaConnectionVM() { TestEnvironment = rexExecutionVM.TestEnvironment, ApiKey = rexExecutionVM.ApiKey, ApiSecret = rexExecutionVM.ApiSecret});
            data.RexUsers = this.PersonaRexDAO.GetPersona(rexExecutionVM.RexCompanyDomains, rexExecutionVM.RexToken, rexExecutionVM.Paginated, rexExecutionVM.DaysSinceUsersUpdated, versionConfiguration);
            data.Contracts = this.GetCompanyContracts(rexExecutionVM);
            List<Contrato> activeContracts = this.GetCurrentCompanyContracts(data.Contracts,rexExecutionVM);
            data.RexPositions = GetCargos(rexExecutionVM);
            data.GeoVictoriaPositions = this.PositionBusiness.ProcessPosition(rexExecutionVM, activeContracts, data.RexPositions);
            data.RexGroups = this.GetCatalogItems(rexExecutionVM, activeContracts, rexExecutionVM.MatchGVGroupsWith);
            data.GeoVictoriaGroups = this.GroupBusiness.ProcessGroups(rexExecutionVM, data.RexGroups);
            return data;
        }
                        
        /// <summary>
        /// Recorre en paralelo usuarios obtenidos desde Rex+ para conocer cuales son nuevos, cuales han sido movido de grupo, cuales tienen datos actualizados, cuales deben ser deshabilitados o necesitan volver a habilitarse.
        /// </summary>
        /// <param name="usersRex"></param>
        /// <param name="usersGeoVictoria"></param>
        /// <param name="contracts"></param>
        /// <param name="groupsGeoVictoria"></param>
        /// <param name="offices"></param>
        /// <param name="itemsPositionsRex"></param>
        /// <returns></returns>
        private List<TransactionUserVM> GetUsersToProcess(RexExecutionVM rexExecutionVM, ProcessUserDataObjectVM data)
        {
            IDictionary<string, List<UserVM>> response = new Dictionary<string, List<UserVM>>();
            ConcurrentBag<LogEntity> logEntities = new ConcurrentBag<LogEntity>();
            ConcurrentBag<TransactionUserVM> usersToProcess = new ConcurrentBag<TransactionUserVM>();

            var query = data.RexUsers.GroupBy(x => x.empleado).ToDictionary(g => g.Key, g => g.ToList());
            var duplicatesEmployees = query.Where(group => group.Value.Count() > 1).ToDictionary(p => p.Key, p => p.Value);
            var nonDuplicatesEmployees = query.Where(group => group.Value.Count() == 1).Select(group => group.Value.First()).ToList();

            #region Flujo normal
            Parallel.ForEach(nonDuplicatesEmployees, new ParallelOptions { MaxDegreeOfParallelism = MaxParallel }, (employee) =>
            {
                //Buscamos los contratos del usuario en Rex+
                var userContracts = data.Contracts.FindAll(a => a.empleado == employee.empleado);
                if (userContracts.Any())
                {
                    UserVM uRex = this.ParseRexPersonaToUserVM(employee, userContracts, rexExecutionVM, data);
                    UserVM uGeoVictoria = data.GeoVictoriaUsers.Find(u => (u?.Identifier != null ? u.Identifier.ToUpper() : "") == uRex.Identifier.ToUpper());

                    if (rexExecutionVM.SyncRepeatedMailUsers && RepeatedMail(uRex, uGeoVictoria, data.GeoVictoriaUsers))
                    {
                        uRex.Email = "";
                    }

                    TransactionUserVM transactionUser = this.TransactionUser(uRex, uGeoVictoria, rexExecutionVM);

                    if (transactionUser.Add || transactionUser.Update || transactionUser.Move || transactionUser.Enable || transactionUser.Disable)
                    {
                        usersToProcess.Add(transactionUser);
                    }

                    foreach (var logEntity in transactionUser.LogEntities)
                    {
                        logEntities.Add(logEntity);
                    }
                }
            });
            #endregion

            #region En caso de duplicados
            Parallel.ForEach(duplicatesEmployees, new ParallelOptions { MaxDegreeOfParallelism = MaxParallel }, (employees) =>
            {
                //Buscamos los contratos del usuario en Rex+
                var userContracts = data.Contracts.FindAll(a => a.empleado == employees.Value.FirstOrDefault()?.empleado);
                if (userContracts.Any())
                {
                    UserVM uRex = this.ParseMultipleRexPersonaToSingleUserVM(employees.Value, userContracts, rexExecutionVM, data);
                    UserVM uGeoVictoria = data.GeoVictoriaUsers.Find(u => u.Identifier.ToUpper() == uRex.Identifier.ToUpper());

                    if (rexExecutionVM.SyncRepeatedMailUsers && RepeatedMail(uRex, uGeoVictoria, data.GeoVictoriaUsers))
                    {
                        uRex.Email = "";
                    }

                    TransactionUserVM transactionUser = this.TransactionUser(uRex, uGeoVictoria, rexExecutionVM);
                    
                    if (transactionUser.Add || transactionUser.Update || transactionUser.Move || transactionUser.Enable || transactionUser.Disable)
                    {
                        usersToProcess.Add(transactionUser);
                    }

                    foreach (var logEntity in transactionUser.LogEntities)
                    {
                        logEntities.Add(logEntity);
                    }

                }
            });
            #endregion

            if (rexExecutionVM.DisableNonIntegratedUser)
            {
                var enabledGVUsers = data.GeoVictoriaUsers.FindAll(x => x.Enabled == 1);                
                foreach (var gvUser in enabledGVUsers)
                {
                    if (gvUser.Identifier != null)
                    {
                        var rexFormattedIdentifier = gvUser.Identifier;
                        if (rexExecutionVM.HasIDSeparator)
                        {
                            rexFormattedIdentifier = gvUser.Identifier.Substring(0, gvUser.Identifier.Length - 1) + "-" + gvUser.Identifier.Last();
                        }
                        
                        if (gvUser.UserProfile != "Administrator" && !data.Contracts.Any(x => string.Compare(x.empleado, rexFormattedIdentifier, true) == 0))
                        {
                            gvUser.DisableCause = "Usuario no existe en Rex+";
                            TransactionUserVM transactionUser = new TransactionUserVM()
                            {
                                User = gvUser,
                                Disable = true
                            };
                            usersToProcess.Add(transactionUser);
                        }

                    }
                }
            }

            new TableStorageHelper().Upsert<LogEntity>(logEntities.ToList(), LogTable.NAME);

            return usersToProcess.ToList();
        }

        /// <summary>
        /// Convierte un objeto de clase Empleado a UserVM, juntando los datos desde Contratos, Grupos y Sedes en caso de ser necesario
        /// </summary>
        /// <param name="userRex"></param>
        /// <param name="contract"></param>
        /// <param name="groups"></param>
        /// <param name="offices"></param>
        /// <returns></returns>
        private UserVM ParseRexPersonaToUserVM(Empleado userRex, List<Contrato> userContracts, RexExecutionVM rexExecutionVM, ProcessUserDataObjectVM data)
        {   
            UserVM user = new UserVM()
            {
                Identifier = GeneralHelper.ParseIdentifier(userRex.empleado).ToUpper(),
                Name = userRex.nombre,
                LastName = userRex.apellidoPate + " " + userRex.apellidoMate,
                Phone = userRex.numeroFono,
                Email = userRex.emailPersonal,
                Custom1 = userRex.contratoActi
            };

            #region Info basica
            user.Adress = $"{userRex.direccion} {userRex.ciudad}";
            if (user.Adress.Length > 100)
            {
                user.Adress = user.Adress.Substring(0, 100);
            }

            if (user.Email.ToLower().Contains(UserDefaultValues.Email))
            {
                user.Email = null;
            }

            if (!string.IsNullOrWhiteSpace(userRex.empresa))
            {
                user.UserCompanyIdentifier = GeneralHelper.ParseIdentifier(data.RexCompanies.FirstOrDefault(x => x.empresa == userRex.empresa)?.rut);
            }
            else
            {
                user.UserCompanyIdentifier = null;
            }
            #endregion

            #region Contrato
            var contract = (Contrato)null;
            if (rexExecutionVM.UtilizaAsistencia)
            {
                contract = userContracts.FirstOrDefault(x => x.contrato == userRex.contratoActi && x.empresa == userRex.empresa && x.utiliza_asistencia);
            }
            else
            {
                contract = userContracts.FirstOrDefault(x => x.contrato == userRex.contratoActi && x.empresa == userRex.empresa);
            }
            if (contract == null)
            {
                try
                {
                    if (userContracts.Count > 0)
                    {
                        contract = userContracts.OrderByDescending(x => int.Parse(x.contrato)).FirstOrDefault();
                        if (rexExecutionVM.UtilizaAsistencia && !contract.utiliza_asistencia)
                        {
                            //si el ultimo contrato vigente en Rex no utiliza asistencia, no se debe traspasar el usuario a GV
                            user.Enabled = 0;
                            return user;
                        }
                    }
                    else
                    {
                        //si no tiene contrato en Rex, no puede crearse en GV por ende, se retorna el usuario como deshabilitado
                        user.Enabled = 0;
                        return user;
                    }
                }
                catch (Exception)
                {
                    //En caso de problemas con el contrato, no puede crearse en GV por ende, se retorna el usuario como deshabilitado
                    user.Enabled = 0;
                    return user;
                }
            }

            user.StartContractDate = DateTimeHelper.StringDateTimeFileToDateTime(contract.fechaInic);
            user.EndContractDate = string.IsNullOrWhiteSpace(contract.fechaTerm) ? new DateTime(1, 1, 1, 0, 0, 0) : DateTimeHelper.StringDateTimeFileToDateTime(contract.fechaTerm);
            user.Causal = contract.causal;
            #endregion

            #region Cargo
            ObjetoCatalogo positionRex;

            if (rexExecutionVM.OrgDevelopment)
            {
                positionRex = data.RexPositions.FirstOrDefault(a => a.id == contract.cargo_id);
            }
            else
            {
                positionRex = data.RexPositions.FirstOrDefault(a => a.item == contract.cargo);
            }

            if (positionRex != null)
            {
                string positionName = positionRex.nombre.Trim().ToLower();
                foreach (var positionGV in data.GeoVictoriaPositions)
                {
                    if (positionGV.PositionDescription.Trim().ToLower() == positionName)
                    {
                        user.PositionIdentifier = positionGV.Identifier;
                        break;
                    }
                }
            }
            #endregion

            #region Grupo
            GroupApiVM group = null;
            switch (rexExecutionVM.MatchGVGroupsWith)
            {
                case UserGroupOrigin.COST_CENTER:
                    user.ContractGroupCode = contract.centroCost;
                    group = data.GeoVictoriaGroups.FirstOrDefault(x => x.CostCenter == contract.centroCost);
                    break;
                case UserGroupOrigin.OFFICE:
                    user.ContractGroupCode = contract.sede;
                    group = data.GeoVictoriaGroups.FirstOrDefault(x => x.CostCenter == contract.sede);
                    break;
                default:
                    break;
            }

            
            if (group != null && group.CostCenter != null)
            {
                user.GroupIdentifier = group.CostCenter;
                user.GroupDescription = group.Description;
            }
            #endregion

            user.Enabled = 1;
            
            if (rexExecutionVM.DisableUserIfContractEnds && user.EndContractDate > new DateTime(1, 1, 1, 0, 0, 0) && user.EndContractDate < DateTime.Today.Date)
            {
                user.Enabled = 0;
                user.DisableCause = $"Contrato ha expirado";
            }

            if (rexExecutionVM.DisableUserIfContractIsZero && userRex.contratoActi == "0")
            {
                user.Enabled = 0;
                user.DisableCause = $"No tiene contrato activo";
            }

            if (rexExecutionVM.DisableUserIfSituacionF && userRex.situacion == "F")
            {
                user.Enabled = 0;
                user.DisableCause = $"Situación es finiquitado";
            }

            if (rexExecutionVM.DisableUserIfSituacionS && userRex.situacion == "S")
            {
                user.Enabled = 0;
                user.DisableCause = $"Situación es suspendido";
            }

            if (!rexExecutionVM.RexCompanyCodes.Contains(userRex.empresa))
            {
                user.Enabled = 0;
                user.DisableCause = $"Corresponde a otra empresa";
            }

            if (rexExecutionVM.DisableUserIfModalidadContratoS && contract.modalidad_contrato == "S")
            {
                user.Enabled = 0;
                user.DisableCause = $"Se encuentra bajo Art. 22";
            }
            user.Custom2 = contract.id;
            return user;
        }

        private UserVM ParseMultipleRexPersonaToSingleUserVM(List<Empleado> usersRex, List<Contrato> userContracts, RexExecutionVM rexExecutionVM, ProcessUserDataObjectVM data)
        {
            List<UserVM> parsedUsers = new List<UserVM>();
            foreach (Empleado userRex in usersRex)
            {
                var contractsMatchingURL = userContracts.FindAll(x => x.RexDomain == userRex.RexDomain);
                if (contractsMatchingURL.Count > 0)
                {
                    parsedUsers.Add(this.ParseRexPersonaToUserVM(userRex, userContracts, rexExecutionVM, data));
                }
            }

            //Obtenemos el primer usuario de la lista, empezando con el que se encuentre habilitado.
            var user = parsedUsers.OrderByDescending(x => x.Enabled).FirstOrDefault();
            
            return user;
        }

        private TransactionUserVM TransactionUser(UserVM uRex, UserVM uGeoVictoria, RexExecutionVM rexExecutionVM)
        {
            TransactionUserVM transactionUser = new TransactionUserVM()
            {
                User = uRex,
                LogEntities = new List<LogEntity>()
            };

            if (uGeoVictoria == null)
            {
                if (rexExecutionVM.AssignNewUsersToTempGroup && string.IsNullOrWhiteSpace(uRex.GroupIdentifier))
                {
                    uRex.GroupIdentifier = DefaultGroup.Identifier;
                }

                if (uRex.Enabled == 1)
                {
                    if (!string.IsNullOrWhiteSpace(uRex.GroupIdentifier))
                    {
                        //Si es que el usuario no existe en GV y el grupo al que pertenece (segun Rex+) existe
                        transactionUser.Add = true;
                    }
                    else
                    {
                        transactionUser.LogEntities.Add(LogEntityHelper.User(rexExecutionVM, LogEvent.ADD, uRex.Identifier, $"No se pudo agregar usuario a un grupo con centro de costo '{uRex.ContractGroupCode}'", LogType.Warning));
                    }
                }
                
            }
            else
            {
                bool activeGV = (uGeoVictoria.Enabled == 1) ? true : false;
                bool activeRex = (uRex.Enabled == 1) ? true : false;
                if (activeGV)
                {
                    (bool needsUpdate, string modificationCause) = this.NeedsUpdate(uRex, uGeoVictoria);

                    if (needsUpdate)
                    {
                        transactionUser.Update = needsUpdate;
                        transactionUser.User.UpdateCause = modificationCause;
                    }

                    if (activeRex && uRex.GroupIdentifier == null && !string.IsNullOrEmpty(rexExecutionVM.MatchGVGroupsWith))
                    {
                        transactionUser.LogEntities.Add(LogEntityHelper.User(rexExecutionVM, LogEvent.NONE, uRex.Identifier, $"No se pudo asociar a un grupo con centro de costo '{uRex.ContractGroupCode}'", LogType.Warning));
                    }

                    //Si es que el usuario existe en GV y el grupo al que pertenece es diferente entre Rex+ y GV, se mueve.
                    if (rexExecutionVM.MoveUsers && !string.IsNullOrWhiteSpace(uRex.GroupIdentifier) && uGeoVictoria.GroupIdentifier != uRex.GroupIdentifier)
                    {
                        uRex.OldGroupDescription = uGeoVictoria.GroupDescription;
                        transactionUser.Move = true;
                    }

                    //Si el usuario Rex no esta activo y no esta en el listado de "Excluir de Desactivados", se desactiva.
                    //Si el causal del usuario Rex no esta en el listado "CausalCode", se desactiva
                    List<string> causalCodeListRex = GeneralHelper.ValidateStringList(rexExecutionVM.CausalCode);
                    if (!activeRex &&
                        uGeoVictoria.UserProfile != "Administrator" &&
                        !rexExecutionVM.ExcludeFromDisable.Contains(uRex.Identifier.ToUpper()) && !causalCodeListRex.Contains(uRex.Causal))
                    {
                        transactionUser.Disable = true;
                    }

                }
                else if (!activeGV && activeRex)
                {
                    //Si el usuario está deshabilitado en GV, se habilita
                    transactionUser.Enable = true;
                }

            }

            return transactionUser;
        }

        private (bool needsUpdate, string modificationCause) NeedsUpdate(UserVM uRex, UserVM uGeoVictoria)
        {
            bool needsUpdate = false;
            string modificationCause = string.Empty;
            List<string> propertiesToUpdate = new List<string>();

            //Si es que el usuario tiene datos diferentes entre Rex+ y GV, se actualiza.
            if (uRex.Name != uGeoVictoria.Name)
            {
                propertiesToUpdate.Add("nombre");
            }

            if (uRex.LastName != uGeoVictoria.LastName)
            {
                propertiesToUpdate.Add("apellido");
            }

            if (uRex.Custom1 != uGeoVictoria.Custom1)
            {
                propertiesToUpdate.Add("personalizado 1");
            }

            if (uRex.Custom2 != uGeoVictoria.Custom2)
            {
                propertiesToUpdate.Add("personalizado 2");
            }

            if (uRex.StartContractDate != uGeoVictoria.StartContractDate)
            {
                propertiesToUpdate.Add("fecha inicio de contrato");
            }

            if (uRex.EndContractDate != uGeoVictoria.EndContractDate)
            {
                if (!(uRex.EndContractDate == nullDateTime && uGeoVictoria.EndContractDate == null))
                {
                    propertiesToUpdate.Add("fecha fin de contrato");
                    Console.WriteLine($"{uRex.EndContractDate} -> {uGeoVictoria.EndContractDate}");
                }
            }

            if (uRex.PositionIdentifier != uGeoVictoria.PositionIdentifier)
            {
                propertiesToUpdate.Add("cargo");
            }

            if (uRex.UserCompanyIdentifier != uGeoVictoria.UserCompanyIdentifier)
            {
                propertiesToUpdate.Add("razón social");
            }

            if (!string.IsNullOrWhiteSpace(uRex.Email) && !uRex.Email.Trim().Equals(uGeoVictoria.Email, StringComparison.OrdinalIgnoreCase))
            {
                propertiesToUpdate.Add("email");
            }

            if (propertiesToUpdate.Count > 0)
            {
                needsUpdate = true;
                modificationCause = $"Se modifica " + string.Join(", ", propertiesToUpdate);
            }

            return (needsUpdate, modificationCause);
        }

        private List<Contrato> GetCompanyContracts(RexExecutionVM rexExecutionVM)
        {
            List<Contrato> contracts = this.PersonaRexDAO.GetContratos(rexExecutionVM.RexCompanyDomains, rexExecutionVM.RexToken, rexExecutionVM.Paginated, rexExecutionVM.DaysSinceUsersUpdated, rexExecutionVM.DisableUserIfContractEnds, versionConfiguration);
            return contracts;
        }

        private List<Contrato> GetCurrentCompanyContracts(List<Contrato> contracts, RexExecutionVM rexExecutionVM)  
        {
            List<Contrato> activeContracts = contracts.FindAll(a => a.estado == "A");
            activeContracts = (rexExecutionVM.UtilizaAsistencia) ? activeContracts.FindAll(x => x != null && rexExecutionVM.RexCompanyCodes.Contains(x.empresa)&& x.utiliza_asistencia)
                                                           : activeContracts.FindAll(x => x != null && rexExecutionVM.RexCompanyCodes.Contains(x.empresa));

            return activeContracts;
        }

        public List<Contrato> GetCurrentCompanyContractsByDate(RexExecutionVM rexExecutionVM,DateTime startProcessDate, DateTime endProcessDate, bool activeOnly = true)
        {
            List<Contrato> contracts = this.PersonaRexDAO.GetContractsFromDates(rexExecutionVM.RexCompanyDomains, rexExecutionVM.RexToken, rexExecutionVM.Paginated, startProcessDate, endProcessDate, versionConfiguration);
            if (activeOnly)
            {
                contracts = contracts.FindAll(a => a.estado == "A");//contratos solo activos en rex
            }            
            contracts = (rexExecutionVM.UtilizaAsistencia) ? contracts.FindAll(x => x != null && rexExecutionVM.RexCompanyCodes.Contains(x.empresa)&& x.utiliza_asistencia)
                                                           : contracts.FindAll(x => x != null && rexExecutionVM.RexCompanyCodes.Contains(x.empresa));
            return contracts;
        }

        public List<Cotizaciones> GetQuote(RexExecutionVM rexExecutionVM)
        {
            List<Cotizaciones> cotizaciones = new List<Cotizaciones>();
            List<LogEntity> logEntities = new List<LogEntity>();

            try
            {
                var cotizacionesList = this.ClienteExternoDAO.GetPriceRequests(rexExecutionVM.RexCompanyDomains, rexExecutionVM.RexToken, this.versionConfiguration);

                if (cotizacionesList == null || !cotizacionesList.Any())
                {
                    return cotizaciones;
                }

                cotizaciones.AddRange(cotizacionesList);
            }
            catch (Exception ex)
            {
                logEntities.Add(LogEntityHelper.LogEntity(
                    rexExecutionVM,
                    LogEvent.GET,
                    string.Join(", ", rexExecutionVM.RexCompanyDomains),
                    $"Error al obtener o procesar la cotización: {ex.Message}",
                    LogType.Error,
                    LogItem.COTIZACIONES
                ));

                throw;
            }

            new TableStorageHelper().Upsert<LogEntity>(logEntities.ToList(), LogTable.NAME);
            return cotizaciones;
        }
        /// <summary>
        /// Se crean los contratos que no existen, y se actualizan los que si existen, pero tienen diferencias.
        /// </summary>
        /// <param name="users"></param>
        /// <param name="contracts"></param>
        /// <param name="rexExecutionVM"></param>
        /// <param name="cotizaciones"></param>
        private void SendContractsToApi(List<UserVM> users, List<Contrato> contracts, RexExecutionVM rexExecutionVM, List<Cotizaciones> cotizaciones)
        {
            List<ContractClientVM> contractsToSend = new List<ContractClientVM>();
            List<ContractDTO> contractsToUpdate = new List<ContractDTO>();
            List<LogEntity> logEntities = new List<LogEntity>();

            List<string> userIdentifiers = users.Where(u => u?.Identifier != null).ToList().ConvertAll(u => u.Identifier);

            var contractFilter = new ContractFilterVM
            {
                UserIdentifiers = userIdentifiers
            };

            List<ContractDTO> existingContracts = this.GetExistingContracts(rexExecutionVM, contractFilter);

            foreach (var usuario in users)
            {
                try
                {
                    if (usuario == null)
                    {
                        string message = "Usuario es null.";
                        logEntities.Add(LogEntityHelper.LogEntity(rexExecutionVM, "SendContractsToApi", "Usuario", message, "Warning", "Usuario"));
                        LogHelper.Log(message);
                        continue;
                    }

                    if (usuario.Identifier == null)
                    {
                        string message = $"El usuario con Identifier {usuario.Identifier} tiene Identifier null.";
                        LogHelper.Log(message);
                        continue;
                    }

                    var userContracts = contracts.Where(c => usuario.Identifier == GeneralHelper.ParseIdentifier(c.empleado)?.ToUpper()).ToList();

                    if (!userContracts.Any())
                    {
                        string message = $"No se encontraron contratos asociados para el usuario {usuario.Identifier}. No se asignará ningún contrato por defecto.";
                        LogHelper.Log(message);
                        continue;
                    }

                    foreach (var contrato in userContracts)
                    {
                        if (contrato == null)
                        {
                            string message = "Contrato es null.";
                            LogHelper.Log(message);
                            continue;
                        }

                        if (contrato.cotizacion == null)
                        {
                            string message = $"El contrato {contrato.id} tiene cotizacion null.";
                            LogHelper.Log(message);
                            continue;
                        }

                        var cotizacion = cotizaciones.FirstOrDefault(c => c.project_contract_id == contrato.cotizacion);

                        if (cotizacion == null)
                        {
                            string message = $"No se encontraron cotizaciones asociadas para el contrato {contrato.id}.";
                            LogHelper.Log(message);
                            continue;
                        }

                        if (existingContracts == null || !existingContracts.Any(ec => ec.ExternalContractId == contrato.contrato && ec.UserIdentifier == usuario.Identifier))
                        {
                            ContractClientVM contractToSend = new ContractClientVM
                            {
                                UserIdentifier = usuario.Identifier,
                                StartDate = DateTimeHelper.StringDateTimeFileToDateTime(contrato.fechaInic).Value,
                                EndDate = contrato.fechaTerm != null ? DateTimeHelper.StringDateTimeFileToDateTime(contrato.fechaTerm) : default,
                                ExternalContractId = contrato.contrato,
                                ExternalClientId = cotizacion.project_id,
                                ClientName = cotizacion.project_name,
                                Status = ParseContractStatus(contrato.estado)
                            };
                            contractsToSend.Add(contractToSend);
                        }
                        else
                        {
                            var existingContract = existingContracts.First(ec => ec.ExternalContractId == contrato.contrato && ec.UserIdentifier == usuario.Identifier);
                            if (ContractMustBeUpdated(existingContract, contrato, cotizacion))
                            {
                                existingContract.StartDate = DateTimeHelper.StringDateTimeFileToDateTime(contrato.fechaInic).Value;
                                existingContract.EndDate = contrato.fechaTerm != null ? DateTimeHelper.StringDateTimeFileToDateTime(contrato.fechaTerm) : default;
                                existingContract.ExternalClientId = cotizacion.project_id;
                                existingContract.Status = ParseContractStatus(contrato.estado);
                                contractsToUpdate.Add(existingContract);
                            }
                            else
                            {
                                string message = $"El contrato {contrato.contrato} ya existe en la base de datos y no requiere actualización.";
                                LogHelper.Log(message);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    string message = $"Error procesando el usuario {usuario?.Identifier ?? "desconocido"}: {ex.Message}";
                    logEntities.Add(LogEntityHelper.LogEntity(rexExecutionVM, "SendContractsToApi", usuario?.Identifier ?? "desconocido", message, "Error", "Usuario"));
                    LogHelper.Log(message);
                    if (ex is ArgumentNullException argEx)
                    {
                        string nullParamMessage = $"Parámetro nulo: {argEx.ParamName}";
                        logEntities.Add(LogEntityHelper.LogEntity(rexExecutionVM, "SendContractsToApi", usuario?.Identifier ?? "desconocido", nullParamMessage, "Error", "Usuario"));
                        LogHelper.Log(nullParamMessage);
                    }
                }
            }

            var contractsToProcess = new ContractContainerVM
            {
                ToCreate = contractsToSend,
                ToUpdate = contractsToUpdate
            };

            this.ProcessContracts(contractsToProcess, rexExecutionVM);

            new TableStorageHelper().Upsert<LogEntity>(logEntities.ToList(), LogTable.NAME);
        }

        /// <summary>
        /// Envía los contratos para su creación o modificación
        /// </summary>
        /// <param name="contracts"></param>
        /// <param name="rexExecution"></param>
        private void ProcessContracts(ContractContainerVM contracts, RexExecutionVM rexExecution)
        {
            try
            {
                if (contracts.ToCreate.Any() || contracts.ToUpdate.Any())
                {

                    this.ContractGeoVictoriaDAO.ProcessContracts(new GeoVictoriaConnectionVM()
                    {
                        ApiKey = rexExecution.ApiKey,
                        ApiSecret = rexExecution.ApiSecret,
                        ApiToken = rexExecution.ApiToken,
                        TestEnvironment = rexExecution.TestEnvironment
                    }, contracts);
                }
            }
            catch (AggregateException ae)
            {
                foreach (var ex in ae.InnerExceptions)
                {
                    LogHelper.Log($"Error procesando contratos: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error general: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene los contratos de los usuarios a procesar
        /// </summary>
        /// <param name="rexExecution"></param>
        /// <param name="contractFilter"></param>
        /// <returns></returns>
        private List<ContractDTO> GetExistingContracts(RexExecutionVM rexExecution, ContractFilterVM contractFilter)
        {
            return this.ContractGeoVictoriaDAO.FindUserContracts(new GeoVictoriaConnectionVM()
            {
                ApiKey = rexExecution.ApiKey,
                ApiSecret = rexExecution.ApiSecret,
                ApiToken = rexExecution.ApiToken,
                TestEnvironment = rexExecution.TestEnvironment
            }, contractFilter);
        }

        private byte ParseContractStatus(string status) =>
            status switch
            {
                "A" => 1,
                _ => 0
            };

        private bool ContractMustBeUpdated(ContractDTO existingContract, Contrato contrato, Cotizaciones cotizacion)
        {
            return existingContract.StartDate != DateTimeHelper.StringDateTimeFileToDateTime(contrato.fechaInic) ||
                   existingContract.EndDate != (contrato.fechaTerm != null ? DateTimeHelper.StringDateTimeFileToDateTime(contrato.fechaTerm) : default ) ||
                   existingContract.ExternalClientId != cotizacion.project_id ||
                   existingContract.Status != ParseContractStatus(contrato.estado);
        }
        private List<ObjetoCatalogo> GetCatalogItems(RexExecutionVM rexExecutionVM, List<Contrato> contracts, string catalogItem)
        {
            List<ObjetoCatalogo> items = null;
            List<string> itemIDs = null;
            switch (catalogItem)
            {
                case UserGroupOrigin.COST_CENTER:
                    itemIDs = contracts.Select(x => x.centroCost).Distinct().ToList();
                    items = this.CatalogoRexDAO.GetCentrosCosto(rexExecutionVM.RexCompanyDomains, rexExecutionVM.RexToken, versionConfiguration);
                    break;
                case UserGroupOrigin.OFFICE:
                    itemIDs = contracts.Select(x => x.sede).Distinct().ToList();
                    items = this.CatalogoRexDAO.GetSedes(rexExecutionVM.RexCompanyDomains, rexExecutionVM.RexToken, versionConfiguration);
                    break;
                default:
                    return new List<ObjetoCatalogo>();
            }
            
            items = items.FindAll(x => itemIDs.Contains(x.item));

            return items;
        }

        /// <summary>
        /// Método encargado de procesar los diferentes usuarios dependiendo si son para
        /// agregar, mover de grupo, modificar, habilitar, y deshabilitar.
        /// </summary>
        /// <param name="usersToProcess"></param>
        private void ProcessUsers(RexExecutionVM rexExecution, List<TransactionUserVM> usersToProcess, GeoVictoriaConnectionVM gvConnection)
        {
            List<UserVM> toAddUsers = usersToProcess.Where(x => x.Add).Select(x => x.User).ToList();
            List<UserVM> toMoveUsers = usersToProcess.Where(x => x.Move).Select(x => x.User).ToList();
            List<UserVM> toUpdateUsers = usersToProcess.Where(x => x.Update).Select(x => x.User).ToList();
            List<UserVM> toEnableUpdateUsers = usersToProcess.Where(x => x.Enable).Select(x => x.User).ToList();
            List<UserVM> toDisableUpdateUsers = usersToProcess.Where(x => x.Disable).Select(x => x.User).ToList();

            //Si es que estamos ejecutando local o solo para probar, mostramos este resumen
            bool execute = false;
            bool.TryParse(ConfigurationHelper.Value("Execute"), out execute);
            if (!execute)
            {
                LogHelper.Log("Results: Operation --- Process Users: " + toAddUsers.Count + " to Add");
                LogHelper.Log("Results: Operation --- Process Users: " + toMoveUsers.Count + " to Move between groups");
                LogHelper.Log("Results: Operation --- Process Users: " + toUpdateUsers.Count + " to Edit");
                LogHelper.Log("Results: Operation --- Process Users: " + toEnableUpdateUsers.Count + " to Enable");
                LogHelper.Log("Results: Operation --- Process Users: " + toDisableUpdateUsers.Count + " to Disable");
            }
            
            //Add, update and enbable / update users
            this.Add(rexExecution, toAddUsers, gvConnection);

            //Move users between groups
            this.Move(rexExecution, toMoveUsers, gvConnection);

            //Move users between groups
            this.Update(rexExecution, toUpdateUsers, gvConnection);

            //Update(toUpdateUsers);
            this.Enable(rexExecution, toEnableUpdateUsers, gvConnection);

            //Disable users
            this.Disable(rexExecution, toDisableUpdateUsers, gvConnection);
        }

        #region Transactions
        public void Add(RexExecutionVM rexExecution, List<UserVM> users, GeoVictoriaConnectionVM gvConnection)
        {
            List<LogEntity> logEntities = new List<LogEntity>();
            foreach (UserVM user in users)
            {
                (bool success, string message) = this.UserGeoVictoriaDAO.Add(user, gvConnection);
                logEntities.Add(LogEntityHelper.User(
                    rexExecution, 
                    LogEvent.ADD, 
                    user.Identifier, 
                    message,
                    success ? LogType.Info : LogType.Error));
            }
            new TableStorageHelper().Upsert(logEntities, LogTable.NAME);
        }

        public void Update(RexExecutionVM rexExecution, List<UserVM> toUpdateUsers, GeoVictoriaConnectionVM gvConnection)
        {
            List<LogEntity> logEntities = new List<LogEntity>();
            foreach (UserVM user in toUpdateUsers)
            {
                (bool success, string message) = this.UserGeoVictoriaDAO.Update(user, gvConnection);
                logEntities.Add(LogEntityHelper.User(
                    rexExecution,
                    LogEvent.EDIT,
                    user.Identifier,
                    message,
                    success ? LogType.Info : LogType.Error));
            }
            new TableStorageHelper().Upsert(logEntities, LogTable.NAME);
        }

        public void Enable(RexExecutionVM rexExecution, List<UserVM> users, GeoVictoriaConnectionVM gvConnection)
        {
            List<LogEntity> logEntities = new List<LogEntity>();
            foreach (UserVM user in users)
            {
                (bool success, string message) = this.UserGeoVictoriaDAO.Enable(user, gvConnection);
                logEntities.Add(LogEntityHelper.User(
                    rexExecution,
                    LogEvent.ENABLE,
                    user.Identifier,
                    message,
                    success ? LogType.Info : LogType.Error));

            }
            new TableStorageHelper().Upsert(logEntities, LogTable.NAME);
        }

        public void Move(RexExecutionVM rexExecution, List<UserVM> users, GeoVictoriaConnectionVM gvConnection)
        {
            List<LogEntity> logEntities = new List<LogEntity>();
            foreach (UserVM user in users)
            {
                (bool success, string message) = this.UserGeoVictoriaDAO.MoveOfGroup(user, gvConnection);
                logEntities.Add(LogEntityHelper.User(
                    rexExecution,
                    LogEvent.MOVE,
                    user.Identifier,
                    message,
                    success ? LogType.Info : LogType.Error));
            }
            new TableStorageHelper().Upsert(logEntities, LogTable.NAME);
        }

        public void Disable(RexExecutionVM rexExecution, List<UserVM> users, GeoVictoriaConnectionVM gvConnection)
        {
            List<LogEntity> logEntities = new List<LogEntity>();
            foreach (UserVM user in users)
            {
                (bool success, string message) = this.UserGeoVictoriaDAO.Disable(user, gvConnection);
                logEntities.Add(LogEntityHelper.User(
                    rexExecution,
                    LogEvent.DISABLE,
                    user.Identifier,
                    message,
                    success ? LogType.Info : LogType.Error));
            }
            new TableStorageHelper().Upsert(logEntities, LogTable.NAME);
        }
        #endregion

        public List<Empleado> GetEmployeesData(RexExecutionVM rexExecutionVM)
        {
            List<Empleado> employees = this.PersonaRexDAO.GetPersona(rexExecutionVM.RexCompanyDomains, rexExecutionVM.RexToken, false, 0, versionConfiguration);
            List<Contrato> contracts = this.GetCompanyContracts(rexExecutionVM);
            List<Contrato> activeContracts = this.GetCurrentCompanyContracts(contracts,rexExecutionVM);
            List<ObjetoCatalogo> positions = GetCargos(rexExecutionVM);
            List<ObjetoCatalogo> offices = this.GetCatalogItems(rexExecutionVM, activeContracts, UserGroupOrigin.OFFICE);
            List<ObjetoCatalogo> costCenters = this.GetCatalogItems(rexExecutionVM, activeContracts, UserGroupOrigin.COST_CENTER);

            foreach (var employee in employees)
            {
                employee.Contrato = contracts.FirstOrDefault(x => employee.empleado == x.empleado && x.contrato == employee.contratoActi && x.empresa == employee.empresa);
                if (employee.Contrato != null)
                {
                    employee.Sede = offices.FirstOrDefault(x => x.item == employee.Contrato.sede);
                    employee.CentroCosto = costCenters.FirstOrDefault(x => x.item == employee.Contrato.centroCost);

                    if (rexExecutionVM.OrgDevelopment)
                    {
                        employee.Cargo = positions.FirstOrDefault(x => x.id == employee.Contrato.cargo_id);
                    }
                    else
                    {
                        employee.Cargo = positions.FirstOrDefault(x => x.item == employee.Contrato.cargo);
                    }

                    if (rexExecutionVM.DisableUserIfContractEnds && DateTimeHelper.StringDateTimeFileToDateTime(employee.Contrato.fechaTerm) < DateTime.Today.Date)
                    {
                        employee.DisableMessage += $"Contrato ha expirado. ";
                    }

                    if (rexExecutionVM.DisableUserIfContractIsZero && employee.contratoActi == "0")
                    {
                        employee.DisableMessage += $"No tiene contrato activo. ";
                    }

                    if (rexExecutionVM.DisableUserIfSituacionF && employee.situacion == "F")
                    {
                        employee.DisableMessage += $"Situación es finiquitado. ";
                    }

                    if (rexExecutionVM.DisableUserIfSituacionS && employee.situacion == "S")
                    {
                        employee.DisableMessage += $"Situación es suspendido. ";
                    }

                    if (!rexExecutionVM.RexCompanyCodes.Contains(employee.empresa))
                    {
                        employee.DisableMessage += $"Corresponde a otra empresa. ";
                    }

                    if (rexExecutionVM.DisableUserIfModalidadContratoS && employee.Contrato.modalidad_contrato == "S")
                    {
                        employee.DisableMessage += $"Se encuentra bajo Art. 22. ";
                    }
                }
                else if(employee.contratoActi == "0")
                {
                    employee.DisableMessage += $"Sin contrato activo. ";
                }
                else
                {
                    employee.DisableMessage += $"Contrato no encontrado. ";
                }
            }

            return employees;
        }

        public List<UserVM> CopyAdminsToSandbox(RexSandboxSyncVM rexSandboxSync)
        {
            var prodUsers = this.UserGeoVictoriaDAO.GetAll(new GeoVictoriaConnectionVM() { TestEnvironment = false, ApiKey = rexSandboxSync.ApiKey, ApiSecret = rexSandboxSync.ApiSecret });
            var sandboxUsers = this.UserGeoVictoriaDAO.GetAll(new GeoVictoriaConnectionVM() { TestEnvironment = true, ApiKey = rexSandboxSync.SandboxApiKey, ApiSecret = rexSandboxSync.SandboxApiSecret });

            var prodAdmins = prodUsers.Where(x => x.UserProfile == "Administrator" && x.Enabled == 1).Select(x => x.Identifier).ToList();
            var sandboxMissingAdmins = sandboxUsers.FindAll(x => prodAdmins.Contains(x.Identifier) && x.UserProfile != "Administrator");

            foreach (var user in sandboxMissingAdmins)
            {
                ProfileUserContract profile = new ProfileUserContract()
                {
                    Identifier = user.Identifier,
                    UserProfile = "EhKM7eLolc9WkWAmVCjZcw"
                };

                if (user.Enabled == 0)
                {
                    user.Enabled = 1;
                    this.UserGeoVictoriaDAO.Enable(user, new GeoVictoriaConnectionVM() { TestEnvironment = true, ApiKey = rexSandboxSync.SandboxApiKey, ApiSecret = rexSandboxSync.SandboxApiSecret });
                }
                this.UserGeoVictoriaDAO.EditProfile(profile, new GeoVictoriaConnectionVM() { TestEnvironment = true, ApiKey = rexSandboxSync.SandboxApiKey, ApiSecret = rexSandboxSync.SandboxApiSecret });
            }

            return sandboxMissingAdmins;
        }

        public List<ObjetoCatalogo> GetCargos (RexExecutionVM rexExecutionVM)
        {
            List<ObjetoCatalogo> cargos = new List<ObjetoCatalogo>();
            if (rexExecutionVM.OrgDevelopment)
            {
                cargos = this.DesarrolloOrganizacionalRexDAO.GetCargos(rexExecutionVM.RexCompanyDomains, rexExecutionVM.RexToken, versionConfiguration);
            }
            else
            {
                cargos = this.CatalogoRexDAO.GetCargos(rexExecutionVM.RexCompanyDomains, rexExecutionVM.RexToken, versionConfiguration);
            }

            return cargos;
        }

        /// <summary>
        /// Método para verificar si mail del usuario ya existe en GeoVictoria para otro usuario
        /// </summary>
        private bool RepeatedMail(UserVM uRex, UserVM uGeoVictoria, List<UserVM> geoVictoriaUsers)
        {
            if (uRex.Email == null) 
            {
                return false;
            }

            int repetedMailUsers = geoVictoriaUsers.Count(u => (u?.Email != null && u.Email.ToUpper() == uRex.Email.ToUpper()));

            if (uGeoVictoria == null && repetedMailUsers > 0)
            {
                return true;
            }

            else if (uGeoVictoria != null & (uGeoVictoria?.Email == null || uGeoVictoria.Email.ToUpper() != uRex.Email.ToUpper()) && repetedMailUsers > 0)
            {
                return true;
            }
            return false;
        }
    }
}
