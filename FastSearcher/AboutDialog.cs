
using System;
using System.Reflection;

namespace FastSearcher {
    public partial class AboutDialog : Gtk.Dialog {
        public AboutDialog()
        {
            this.Build();
            
            AssemblyTitleAttribute title = (AssemblyTitleAttribute)AssemblyTitleAttribute.GetCustomAttribute(
                            System.Reflection.Assembly.GetExecutingAssembly() , typeof(AssemblyTitleAttribute));

            //AssemblyVersionAttribute version = (AssemblyVersionAttribute)AssemblyVersionAttribute.GetCustomAttribute(
            //                System.Reflection.Assembly.GetExecutingAssembly() , typeof(AssemblyVersionAttribute));

            label2.Markup = "<span size='x-large'>" + title.Title + "</span>"; // + " " + version.Version;

            //AssemblyCompanyAttribute company = (AssemblyCompanyAttribute)AssemblyCompanyAttribute.GetCustomAttribute(
            //                System.Reflection.Assembly.GetExecutingAssembly() , typeof(AssemblyCompanyAttribute));

            AssemblyCopyrightAttribute copyright = (AssemblyCopyrightAttribute)AssemblyCopyrightAttribute.GetCustomAttribute(
                            System.Reflection.Assembly.GetExecutingAssembly() , typeof(AssemblyCopyrightAttribute));
            
            AssemblyDescriptionAttribute description = (AssemblyDescriptionAttribute)AssemblyDescriptionAttribute.GetCustomAttribute(
                            System.Reflection.Assembly.GetExecutingAssembly() , typeof(AssemblyDescriptionAttribute));

            label3.Text = description.Description;
            
            label4.Text = copyright.Copyright;

        }

        protected virtual void OnButtonOkActivated (object sender, System.EventArgs e) {
            this.HideAll();
        }
    }
}

