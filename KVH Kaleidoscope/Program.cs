using System;
using System.Windows.Forms;

namespace Kvh.Kaleidoscope
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var view = new MVC_View();
            var model = new MVC_Model();
            MVC_Controller controller = new MVC_Controller(view, model);
            Application.Run(view);
        }
    }
}