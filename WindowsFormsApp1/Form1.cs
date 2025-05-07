using System;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Diagnostics;
using System.Security.Permissions;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            ConfigureBrowserSecurity(); // Nueva configuración de seguridad
            ConfigureBrowserEmulation();
            InitializeWebBrowserSettings();
            LoadLocalApplication("http://localhost/Gym-System/");
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        private void ConfigureBrowserSecurity()
        {
            try
            {
                // Deshabilitar advertencias de scripts
                Registry.SetValue(
                    @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings\Zones\0",
                    "1400", 0, RegistryValueKind.DWord); // Active Scripting: Enable

                // Permitir controles ActiveX
                Registry.SetValue(
                    @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings\Zones\0",
                    "1201", 0, RegistryValueKind.DWord); // Run ActiveX controls and plug-ins: Enable

                // Deshabilitar modo protegido
                Registry.SetValue(
                    @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings\Zones\0",
                    "2500", 3, RegistryValueKind.DWord); // Disable Protected Mode
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error configurando seguridad del navegador: " + ex.Message);
            }
        }

        private void ConfigureBrowserEmulation()
        {
            try
            {
                string appName = System.IO.Path.GetFileName(Application.ExecutablePath);
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                    @"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION",
                    true))
                {
                    if (key != null)
                    {
                        key.SetValue(appName, 11001, RegistryValueKind.DWord); // IE11 Edge mode
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error configuración emulación: " + ex.Message);
            }
        }

        private void InitializeWebBrowserSettings()
        {
            webBrowser1.ScriptErrorsSuppressed = true; // Oculta errores pero permite ejecución
            webBrowser1.AllowWebBrowserDrop = true;
            webBrowser1.IsWebBrowserContextMenuEnabled = true;
            webBrowser1.WebBrowserShortcutsEnabled = true;
            webBrowser1.AllowNavigation = true;
        }

        private void LoadLocalApplication(string url)
        {
            try
            {
                if (!string.IsNullOrEmpty(url))
                {
                    if (CheckLocalhostAccess())
                    {
                        webBrowser1.Navigate(url);
                    }
                    else
                    {
                        ShowErrorPage("El servidor local no está accesible. Asegúrate que tu servidor web está corriendo.");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorPage("Error al cargar la aplicación: " + ex.Message);
            }
        }

        private bool CheckLocalhostAccess()
        {
            try
            {
                var request = System.Net.WebRequest.Create("http://localhost/");
                using (var response = (System.Net.HttpWebResponse)request.GetResponse())
                {
                    return response.StatusCode == System.Net.HttpStatusCode.OK;
                }
            }
            catch
            {
                return false;
            }
        }

        private void ShowErrorPage(string message)
        {
            string errorHtml = $@"
            <html>
                <head><title>Error</title></head>
                <body style='font-family: Arial; padding: 20px;'>
                    <h1>Error al cargar la aplicación</h1>
                    <p>{message}</p>
                    <p>URL intentada: http://localhost/Gym-System/</p>
                </body>
            </html>";

            webBrowser1.DocumentText = errorHtml;
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            this.Text = webBrowser1.Document?.Title ?? "Gym System - Navegador";
            Debug.WriteLine("Página cargada: " + e.Url.ToString());
        }
    }
}