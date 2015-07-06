namespace Dlp.Framework.Container {

    public interface IRegistration {

        /// <summary>
        /// Method used internally to register the interfaces.
        /// </summary>
        /// <returns>Returns an IRegistrationInfo array with all the data to be registered.</returns>
        IRegistrationInfo[] Register();
    }
}
