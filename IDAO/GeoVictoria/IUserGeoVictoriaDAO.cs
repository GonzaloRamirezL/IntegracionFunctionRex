using Common.DTO.GeoVictoria;
using Common.ViewModels;
using System.Collections.Generic;

namespace IDAO.GeoVictoria
{
    public interface IUserGeoVictoriaDAO
    {
        /// <summary>
        /// Get all users (enabled/disabled) from GeoVictoria
        /// </summary>
        /// <returns></returns>
        List<UserVM> GetAll(GeoVictoriaConnectionVM gvConnection);

        /// <summary>
        /// Add a new user to GeoVictoria
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        (bool success, string message) Add(UserVM user, GeoVictoriaConnectionVM gvConnection);

        /// <summary>
        /// Update user information in GeoVictoria
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        (bool success, string message) Update(UserVM user, GeoVictoriaConnectionVM gvConnection);

        /// <summary>
        /// Move a user from a group to another in GeoVictoria
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        (bool success, string message) MoveOfGroup(UserVM user, GeoVictoriaConnectionVM gvConnection);

        /// <summary>
        /// Enable a user in GeoVictoria
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        (bool success, string message) Enable(UserVM user, GeoVictoriaConnectionVM gvConnection);

        /// <summary>
        /// Disable a user in GeoVictoria
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        (bool success, string message) Disable(UserVM user, GeoVictoriaConnectionVM gvConnection);

        bool EditProfile(ProfileUserContract profile, GeoVictoriaConnectionVM gvConnection);
    }
}
