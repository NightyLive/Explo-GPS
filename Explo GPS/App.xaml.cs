using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
//using Google.WebAnalytics;
using System.Device.Location;
using System;


namespace Explo_GPS
{
    public partial class App : Application
    {
        /// <summary>
        /// Permet d'accéder facilement au frame racine de l'application téléphonique.
        /// </summary>
        /// <returns>Frame racine de l'application téléphonique.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }
        //public const string FlurryApiKeyValue = ""; // API EXPLO GPS DEV
        public const string FlurryApiKeyValue = ""; // API EXPLO GPS FREE
        //public const string GoogleKey = "UA-00000000-1"; // GOOGLE ANALYTICS

        /// <summary>
        /// Constructeur pour l'objet Application.
        /// </summary>
        public App()
        {
            // Gestionnaire global pour les exceptions non interceptées. 
            UnhandledException += Application_UnhandledException;

            // Initialisation Silverlight standard
            InitializeComponent();

            // Initialisation spécifique au téléphone
            InitializePhoneApplication();

            // Affichez des informations de profilage graphique lors du débogage.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Affichez les compteurs de fréquence des trames actuels.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Affichez les zones de l'application qui sont redessinées dans chaque frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Activez le mode de visualisation d'analyse hors production, 
                // qui montre les zones d'une page sur lesquelles une accélération GPU est produite avec une superposition colorée.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Désactivez la détection d'inactivité de l'application en définissant la propriété UserIdleDetectionMode de l'objet
                // PhoneApplicationService de l'application sur Désactivé.
                // Attention :- À utiliser uniquement en mode de débogage. Les applications qui désactivent la détection d'inactivité de l'utilisateur continueront de s'exécuter
                // et seront alimentées par la batterie lorsque l'utilisateur ne se sert pas du téléphone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

        }

        // Code à exécuter lorsque l'application démarre (par exemple, à partir de Démarrer)
        // Ce code ne s'exécute pas lorsque l'application est réactivée
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            FlurryWP7SDK.Api.StartSession(FlurryApiKeyValue);
        }

        // Code à exécuter lorsque l'application est activée (affichée au premier plan)
        // Ce code ne s'exécute pas lorsque l'application est démarrée pour la première fois
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            FlurryWP7SDK.Api.StartSession(FlurryApiKeyValue);
        }

        // Code à exécuter lorsque l'application est désactivée (envoyée à l'arrière-plan)
        // Ce code ne s'exécute pas lors de la fermeture de l'application
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            //FlurryWP7SDK.Api.EndSession();
        }

        // Code à exécuter lors de la fermeture de l'application (par exemple, lorsque l'utilisateur clique sur Précédent)
        // Ce code ne s'exécute pas lorsque l'application est désactivée
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            //FlurryWP7SDK.Api.EndSession();
        }

        // Code à exécuter en cas d'échec d'une navigation
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            AppReportingService.Email_Report_Error(e.Exception, "RootFrame_NavigationFailed: " + e.Uri + "\n" + e.Exception.Message + "\n" + e.Exception.StackTrace);
            try
            {
                FlurryWP7SDK.Api.LogError(DateTime.Now.Hour.ToString("00") + "h" + DateTime.Now.Minute.ToString("00") + ": " + e.Uri + "\n" + e.Exception.Message.ToString(), e.Exception);
                FlurryWP7SDK.Api.LogError(e.Exception.StackTrace.ToString(), e.Exception);
                FlurryWP7SDK.Api.EndSession();
            }
            catch
            {
            }
            /*if (MessageBox.Show("Une erreur est survenue, voulez-vous qu'un rapport soit envoyé ?", "Erreur non gérée", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                FlurryWP7SDK.Api.LogError("RootFrame_NavigationFailed: " + e.Uri, e.Exception);
            }*/
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Échec d'une navigation ; arrêt dans le débogueur
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code à exécuter sur les exceptions non gérées
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            // peut recreer une exception avec l'envoi d'email > 32ko
            AppReportingService.Email_Report_Error(e.ExceptionObject, "Application_UnhandledException");
            try
            {
                FlurryWP7SDK.Api.LogError(DateTime.Now.Hour.ToString("00") + "h" + DateTime.Now.Minute.ToString("00") + ": " + e.ExceptionObject.Message.ToString(), e.ExceptionObject);
                FlurryWP7SDK.Api.LogError(e.ExceptionObject.StackTrace.ToString(), e.ExceptionObject);
                FlurryWP7SDK.Api.EndSession();
            }
            catch
            {
            }
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Une exception non gérée s'est produite ; arrêt dans le débogueur
                System.Diagnostics.Debugger.Break();
            }
        }

        #region Initialisation de l'application téléphonique

        // Éviter l'initialisation double
        private bool phoneApplicationInitialized = false;

        // Ne pas ajouter de code supplémentaire à cette méthode
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Créez le frame, mais ne le définissez pas encore comme RootVisual ; cela permet à l'écran de
            // démarrage de rester actif jusqu'à ce que l'application soit prête pour le rendu.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Gérer les erreurs de navigation
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Garantir de ne pas retenter l'initialisation
            phoneApplicationInitialized = true;
        }

        // Ne pas ajouter de code supplémentaire à cette méthode
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Définir le Visual racine pour permettre à l'application d'effectuer le rendu
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Supprimer ce gestionnaire, puisqu'il est devenu inutile
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion
    }
}