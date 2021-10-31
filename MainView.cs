using System;

using Decal.Adapter.Wrappers;

namespace TreeStats
{
    internal static class MainView
    {
        static MyClasses.MetaViewWrappers.IView View;
        static MyClasses.MetaViewWrappers.IButton btnSendUpdate;
        static MyClasses.MetaViewWrappers.ICheckBox chkAutoMode;
        static MyClasses.MetaViewWrappers.ICheckBox chkSendLocation;
        static MyClasses.MetaViewWrappers.IButton btnAddCharacter;
        static MyClasses.MetaViewWrappers.IButton btnRemoveCharacter;

        static MyClasses.MetaViewWrappers.ICheckBox chkUseAccount;
        static MyClasses.MetaViewWrappers.ITextBox edtAccountName;
        static MyClasses.MetaViewWrappers.ITextBox edtAccountPassword;

        static MyClasses.MetaViewWrappers.ICheckBox chkUseCustomURL;
        static MyClasses.MetaViewWrappers.ITextBox edtCustomURL;

        static MyClasses.MetaViewWrappers.ICheckBox chkSilent;

        public static void ViewInit(string icon_path)
        {
            Logging.LogMessage("ViewInit()");

            //Create view here
            View = MyClasses.MetaViewWrappers.ViewSystemSelector.CreateViewResource(PluginCore.MyHost, "TreeStats.MainView.xml");

            // Set custom icon
            if (View.ViewType == MyClasses.MetaViewWrappers.ViewSystemSelector.eViewSystem.VirindiViewService)
            {
                SetCustomIcon(icon_path);
            }

            btnSendUpdate = (MyClasses.MetaViewWrappers.IButton)View["btnSendUpdate"];
            btnSendUpdate.Hit += new EventHandler(btnSendUpdate_Hit);

            chkAutoMode = (MyClasses.MetaViewWrappers.ICheckBox)View["chkAutoMode"];
            chkAutoMode.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkAutoMode_Change);

            chkSendLocation = (MyClasses.MetaViewWrappers.ICheckBox)View["chkSendLocation"];
            chkSendLocation.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkSendLocation_Change);

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

            chkUseCustomURL = (MyClasses.MetaViewWrappers.ICheckBox)View["chkUseCustomURL"];
            chkUseCustomURL.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkUseCustomURL_Change);

            edtCustomURL = (MyClasses.MetaViewWrappers.ITextBox)View["edtCustomURL"];
            edtCustomURL.Change += new EventHandler<MyClasses.MetaViewWrappers.MVTextBoxChangeEventArgs>(edtCustomURL_Change);

            chkSilent = (MyClasses.MetaViewWrappers.ICheckBox)View["chkSilent"];
            chkSilent.Change += new EventHandler<MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs>(chkSilent_Change);

            // Load UI state from settings

            if (Settings.autoMode == true)
            {
                chkAutoMode.Checked = true;
            }

            if (Settings.sendLocation == true)
            {
                chkSendLocation.Checked = true;
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

            if (Settings.useCustomURL == true)
            {
                chkUseCustomURL.Checked = true;
            }

            if (Settings.customURL != null)
            {
                edtCustomURL.Text = Settings.customURL;
            }

            if (Settings.silent == true)
            {
                chkSilent.Checked = true;
            }
        }

        public static void ViewDestroy()
        {
            btnSendUpdate = null;
            chkAutoMode = null;
            chkSendLocation = null;
            btnAddCharacter = null;
            btnRemoveCharacter = null;
            btnSendUpdate = null;
            btnSendUpdate = null;
            chkUseAccount = null;
            chkSilent = null;

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

        static void chkSendLocation_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            if (e.Checked)
            {
                Settings.SetSendLocation(true);
                Settings.Save();
            }
            else
            {
                Settings.SetSendLocation(false); ;
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




        static void chkUseCustomURL_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            if (e.Checked)
            {
                Settings.useCustomURL = true;
                Settings.Save();
            }
            else
            {
                Settings.useCustomURL = false;
                Settings.Save();
            }
        }
        static void edtCustomURL_Change(object sender, MyClasses.MetaViewWrappers.MVTextBoxChangeEventArgs e)
        {
            Settings.customURL = e.Text;
            Settings.Save();
        }

        static void SetCustomIcon(string icon_path)
        {
            try
            {
                System.Drawing.Bitmap icon_bmp = new System.Drawing.Bitmap(icon_path);
                VirindiViewService.ACImage icon = new VirindiViewService.ACImage(icon_bmp);

                MyClasses.MetaViewWrappers.VirindiViewServiceHudControls.View v = (MyClasses.MetaViewWrappers.VirindiViewServiceHudControls.View)View;
                v.Underlying.Icon = icon;
            }
            catch (Exception ex)
            {
                Logging.LogError(ex);
            }
        }

        static void chkSilent_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            if (e.Checked)
            {
                Settings.silent = true;
                Settings.Save();
            }
            else
            {
                Settings.silent = false;
                Settings.Save();
            }
        }
    }
}