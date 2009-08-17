using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using BugNET.UserInterfaceLayer;
using BugNET.BusinessLogicLayer;
using log4net;
using System.Collections.Generic;

namespace BugNET
{
    public partial class UserProfile : BasePage
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(UserProfile));

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {        
            if (!Page.IsPostBack)
            {
                MembershipUser objUser = ITUser.GetUser(User.Identity.Name);
                litUserName.Text = User.Identity.Name;
                FirstName.Text = WebProfile.Current.FirstName;
                LastName.Text = WebProfile.Current.LastName;
                FullName.Text = WebProfile.Current.DisplayName;
                MyIssuesItems.SelectedValue = WebProfile.Current.MyIssuesPageSize.ToString();
                cbAssigned.Checked = WebProfile.Current.ShowAssignedToMe;
                cbReported.Checked = WebProfile.Current.ShowReportedByMe;
                cbMonitored.Checked = WebProfile.Current.ShowMonitoredByMe;
                cbInProgress.Checked = WebProfile.Current.ShowInProgressByMe;
                cbClosed.Checked = WebProfile.Current.ShowClosedByMe;
                cbResolved.Checked = WebProfile.Current.ShowResolvedByMe;
                ddlPreferredLocale.SelectedValue = WebProfile.Current.PreferredLocale;
                IssueListItems.SelectedValue = WebProfile.Current.IssuesPageSize.ToString();
                if(objUser !=null)
                {
                    UserName.Text =objUser.UserName;
                    Email.Text = objUser.Email;
                }

                objUser = null;
            }

        }

        /// <summary>
        /// Handles the SelectedNodeChanged event of the tvAdminMenu control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void tvAdminMenu_SelectedNodeChanged(object sender, EventArgs e)
        {
            ProfileView.ActiveViewIndex = Convert.ToInt32(tvAdminMenu.SelectedNode.Value);
            switch (ProfileView.ActiveViewIndex)
            {
                case 3:
                    lstAllProjects.DataSource = Project.GetProjectsByMemberUserName(Security.GetUserName());
                    lstAllProjects.DataTextField = "Name";
                    lstAllProjects.DataValueField = "Id";
                    lstAllProjects.DataBind();

                    // Copy selected users into Selected Users List Box
                    List<ProjectNotification> projectNotifications = ProjectNotification.GetProjectNotificationsByUsername(Security.GetUserName());
                    foreach (ProjectNotification currentNotification in projectNotifications)
                    {
                        ListItem matchItem = lstAllProjects.Items.FindByValue(currentNotification.ProjectId.ToString());
                        if (matchItem != null)
                        {
                            lstSelectedProjects.Items.Add(matchItem);
                            lstAllProjects.Items.Remove(matchItem);
                        }
                    }

                    //ProjectNotification.GetProjectNotificationsByUsername(Security.GetUserName());
                    NotificationManager nm = new NotificationManager();
                    CheckBoxList1.DataSource = nm.LoadNotificationTypes().FindAll(delegate(INotificationType t) { return t.Enabled == true;});
                    CheckBoxList1.DataTextField = "Name";
                    CheckBoxList1.DataValueField = "Name";
                    CheckBoxList1.DataBind();
                    string[] notificationTypes = WebProfile.Current.NotificationTypes.Split(';');
                    foreach (string s in notificationTypes) 
                    {
                        ListItem currentCheckBox = CheckBoxList1.Items.FindByText(s);
                        if(currentCheckBox != null)
                            currentCheckBox.Selected=true;

                    }
                   

                    break;
            }
        }

        /// <summary>
        /// Adds the user.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void AddProjectNotification(Object s, EventArgs e)
        {
            //The users must be added to a list first becuase the collection can not
            //be modified while we iterate through it.
            var usersToAdd = new List<ListItem>();

            foreach (ListItem item in lstAllProjects.Items)
                if (item.Selected)
                    usersToAdd.Add(item);


            foreach (var item in usersToAdd)
            {
                ProjectNotification pn = new ProjectNotification(Convert.ToInt32(item.Value),Security.GetUserName());
                if (pn.Save())
                {
                    lstSelectedProjects.Items.Add(item);
                    lstAllProjects.Items.Remove(item);
                }
            }

            lstSelectedProjects.SelectedIndex = -1;
        }

        /// <summary>
        /// Removes the user.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
        protected void RemoveProjectNotification(Object s, EventArgs e)
        {
            //The users must be added to a list first becuase the collection can not
            //be modified while we iterate through it.
            var usersToRemove = new List<ListItem>();

            foreach (ListItem item in lstSelectedProjects.Items)
                if (item.Selected)
                    usersToRemove.Add(item);


            foreach (var item in usersToRemove)
            {
                if (ProjectNotification.DeleteProjectNotification(Convert.ToInt32(item.Value),Security.GetUserName()))
                {
                    lstAllProjects.Items.Add(item);
                    lstSelectedProjects.Items.Remove(item);
                }
            }

            lstAllProjects.SelectedIndex = -1;
        }


        /// <summary>
        /// Handles the Click event of the SaveNotificationsButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void SaveNotificationsButton_Click(object sender, EventArgs e)
        {
            try
            {
                string notificationTypes = string.Empty;

                foreach (ListItem li in CheckBoxList1.Items)
                {
                    if (li.Selected)
                        notificationTypes += li.Value + ";";
                        
                }
                notificationTypes = notificationTypes.TrimEnd(';');
                WebProfile.Current.NotificationTypes = notificationTypes;
                WebProfile.Current.Save();
                Message4.ShowInfoMessage(GetLocalResourceObject("ProfileSaved").ToString());
            }
            catch (Exception ex)
            {
                if (Log.IsErrorEnabled)
                {
                    if (HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated)
                        MDC.Set("user", HttpContext.Current.User.Identity.Name);
                    Log.Error("Profile update error", ex);
                }

                Message4.ShowErrorMessage(GetLocalResourceObject("ProfileUpdateError").ToString());
            }
        }

        /// <summary>
        /// Handles the Click event of the SaveButton control.
        /// </summary>
        /// <param name="s">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void SaveButton_Click(object s, EventArgs e)
        {
            MembershipUser objUser = ITUser.GetUser(User.Identity.Name);

            objUser.Email = Email.Text;
            WebProfile.Current.FirstName = FirstName.Text;
            WebProfile.Current.LastName = LastName.Text;
            WebProfile.Current.DisplayName = FullName.Text;

            try
            {
                WebProfile.Current.Save();
                ITUser.UpdateUser(objUser);
                Message1.ShowInfoMessage(GetLocalResourceObject("ProfileSaved").ToString());

                if (Log.IsInfoEnabled)
                {
                    if (HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated)
                        MDC.Set("user", HttpContext.Current.User.Identity.Name);
                    Log.Info("Profile updated");
                }
            }
            catch(Exception ex)
            {
                if (Log.IsErrorEnabled)
                {
                    if (HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated)
                        MDC.Set("user", HttpContext.Current.User.Identity.Name);
                    Log.Error("Profile update error", ex);
                }

                Message1.ShowErrorMessage(GetLocalResourceObject("ProfileUpdateError").ToString());
            }
            

        }

        /// <summary>
        /// Handles the Click event of the BackButton control.
        /// </summary>
        /// <param name="s">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void BackButton_Click(object s, EventArgs e)
        {
            String url = Request.QueryString["referrerurl"];

            if (!string.IsNullOrEmpty(url))
                Response.Redirect(url);
        }

        /// <summary>
        /// Handles the Click event of the cmdChangePassword control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
        protected void cmdChangePassword_Click(object sender, EventArgs e)
        {
            //lblMessage.Visible = false;
            if (cvPasswords.IsValid)
            {
                MembershipUser objUser = ITUser.GetUser(User.Identity.Name);
                if (objUser != null)
                {
                    try
                    {
                        objUser.ChangePassword(objUser.GetPassword(), NewPassword.Text);
                        Message2.ShowInfoMessage(GetLocalResourceObject("PasswordChanged").ToString());

                        if (Log.IsInfoEnabled)
                        {
                            if (HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated)
                                MDC.Set("user", HttpContext.Current.User.Identity.Name);
                            Log.Info("Password changed");
                        }
                    }
                    catch (Exception ex)
                    {
                        if (Log.IsErrorEnabled)
                        {
                            if (HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated)
                                MDC.Set("user", HttpContext.Current.User.Identity.Name);
                            Log.Error("Password update failure", ex);
                        }
                        Message2.ShowErrorMessage(GetLocalResourceObject("PasswordChangeError").ToString());
                    }

                }
            }

        }

        /// <summary>
        /// Handles the Click event of the SaveCustomizeSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void SaveCustomSettings_Click(object sender, EventArgs e)
        {
            WebProfile.Current.MyIssuesPageSize = Convert.ToInt32(MyIssuesItems.SelectedValue);
            WebProfile.Current.ShowAssignedToMe = cbAssigned.Checked;
            WebProfile.Current.ShowReportedByMe = cbReported.Checked;
            WebProfile.Current.ShowMonitoredByMe = cbMonitored.Checked;
            WebProfile.Current.ShowInProgressByMe = cbInProgress.Checked;
            WebProfile.Current.ShowClosedByMe = cbClosed.Checked;
            WebProfile.Current.ShowResolvedByMe = cbResolved.Checked;
            WebProfile.Current.IssuesPageSize = Convert.ToInt32(IssueListItems.SelectedValue);
            WebProfile.Current.PreferredLocale = ddlPreferredLocale.SelectedValue;

            try
            {
                WebProfile.Current.Save();
                Message3.ShowInfoMessage("Your custom settings have been updated successfully.");

                if (Log.IsInfoEnabled)
                {
                    if (HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated)
                        MDC.Set("user", HttpContext.Current.User.Identity.Name);
                    Log.Info("Profile updated");
                }
            }
            catch (Exception ex)
            {
                if (Log.IsErrorEnabled)
                {
                    if (HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated)
                        MDC.Set("user", HttpContext.Current.User.Identity.Name);
                    Log.Error("Profile update error", ex);
                }
                Message3.ShowErrorMessage("Your custom settings could not be updated.");

            }

        }
    }
}
