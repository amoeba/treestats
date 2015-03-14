using System;
using System.Collections.Generic;
using System.Text;

using Decal.Adapter.Wrappers;

namespace TreeStats
{
    internal static class MainView
    {
        static MyClasses.MetaViewWrappers.IView View;
        static MyClasses.MetaViewWrappers.IButton btnSendUpdate;
        static MyClasses.MetaViewWrappers.ICheckBox chkAutoMode;
        static MyClasses.MetaViewWrappers.IButton btnAddCharacter;
        static MyClasses.MetaViewWrappers.IButton btnRemoveCharacter;

        public static void ViewInit()
        {
            Logging.LogMessage("ViewInit()");

            //Create view here
            View = MyClasses.MetaViewWrappers.ViewSystemSelector.CreateViewResource(PluginCore.MyHost, "TreeStats.ViewXML.MainView.xml");

            btnSendUpdate = (MyClasses.MetaViewWrappers.IButton)View["btnSendUpdate"];
            btnSendUpdate.Hit += new EventHandler(btnSendUpdate_Hit);

            chkAutoMode = (MyClasses.MetaViewWrappers.ICheckBox)View["chkAutoMode"];
            chkAutoMode.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkAutoMode_Change);

            btnAddCharacter = (MyClasses.MetaViewWrappers.IButton)View["btnAddCharacter"];
            btnAddCharacter.Hit += new EventHandler(btnAddCharacter_Hit);

            btnRemoveCharacter = (MyClasses.MetaViewWrappers.IButton)View["btnRemoveCharacter"];
            btnRemoveCharacter.Hit += new EventHandler(btnRemoveCharacter_Hit);

            if (Settings.autoMode == true)
            {
                chkAutoMode.Checked = true;
            }
        }

        public static void ViewDestroy()
        {
            btnSendUpdate = null;

            View.Dispose();
        }

        static void btnSendUpdate_Hit(object sender, EventArgs e)
        {
            Character.DoUpdate();
        }
        
        static void chkAutoMode_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            if (e.Checked)
            {
                Settings.autoMode = true;
                Settings.Save();
            }
            else
            {
                Settings.autoMode = false;
                Settings.Save();
            }
        }

        static void btnAddCharacter_Hit(object sender, EventArgs e)
        {
            Settings.AddCharacter(PluginCore.MyCore.CharacterFilter.Server + "-" + PluginCore.MyCore.CharacterFilter.Name);
            Settings.Save();
        }

        static void btnRemoveCharacter_Hit(object sender, EventArgs e)
        {
            Settings.RemoveCharacter(PluginCore.MyCore.CharacterFilter.Server + "-" + PluginCore.MyCore.CharacterFilter.Name);
            Settings.Save();
        }
    }
}
