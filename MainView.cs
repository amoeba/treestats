using System;

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

        static MyClasses.MetaViewWrappers.ICheckBox chkUseAccount;
        static MyClasses.MetaViewWrappers.ITextBox edtAccountName;
        static MyClasses.MetaViewWrappers.ITextBox edtAccountPassword;

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

            btnSendUpdate = (MyClasses.MetaViewWrappers.IButton)View["btnAccountCreate"];
            btnSendUpdate.Hit += new EventHandler(btnAccountCreate_Hit);
            
            btnSendUpdate = (MyClasses.MetaViewWrappers.IButton)View["btnAccountLogin"];
            btnSendUpdate.Hit += new EventHandler(btnAccountLogin_Hit);

            chkUseAccount = (MyClasses.MetaViewWrappers.ICheckBox)View["chkUseAccount"];
            chkUseAccount.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkUseAccount_Change);

            edtAccountName = (MyClasses.MetaViewWrappers.ITextBox)View["edtAccountName"];
            edtAccountPassword = (MyClasses.MetaViewWrappers.ITextBox)View["edtAccountPassword"];

            // Load UI state from settings

            if (Settings.autoMode == true)
            {
                chkAutoMode.Checked = true;
            }

            if (Settings.useAccount == true)
            {
                chkUseAccount.Checked = true;
            }

            if (Settings.accountName != null)
            {
                edtAccountName.Text = Settings.accountName;
            }

            if (Settings.accountPassword != null)
            {
                edtAccountPassword.Text = Settings.accountPassword;
            }
        }

        public static void ViewDestroy()
        {
            btnSendUpdate = null;
            chkAutoMode = null;
            btnAddCharacter = null;
            btnRemoveCharacter = null;
            btnSendUpdate = null;
            btnSendUpdate = null;
            chkUseAccount = null;

            View.Dispose();
        }

        static void btnSendUpdate_Hit(object sender, EventArgs e)
        {
            Character.TryUpdate(true);
        }

        static void chkAutoMode_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            if (e.Checked)
            {
                Settings.SetAutoMode(true);
                Settings.Save();
            }
            else
            {
                Settings.SetAutoMode(false); ;
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


        static void chkUseAccount_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            if (e.Checked)
            {
                Settings.useAccount = true;
                Settings.Save();
            }
            else
            {
                Settings.useAccount = false;
                Settings.Save();
            }
        }

        static void btnAccountLogin_Hit(object sender, EventArgs e)
        {
            Account.Login(edtAccountName.Text, edtAccountPassword.Text);
        }

        static void btnAccountCreate_Hit(object sender, EventArgs e)
        {
            Account.Create(edtAccountName.Text, edtAccountPassword.Text);
        }

    }
}