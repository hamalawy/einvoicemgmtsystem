﻿using System;
using System.Collections;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Data;
using Rss;
using BugNET.BusinessLogicLayer;
using System.Collections.Generic;


namespace BugNET
{
    public partial class Rss : System.Web.UI.Page
    {
        int projectId = 0;
        int channelId = 0;
        string projectName = string.Empty;
        RssChannel channel;
        RssFeed feed;
        List<Issue> colIssues = null;

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            //get project id
            if (Request.QueryString["pid"] != null)
                projectId = Convert.ToInt32(Request.Params["pid"]);
            //get feed id
            if (Request.QueryString["channel"] != null)
                channelId = Convert.ToInt32(Request.Params["channel"]);

            //Security Checks
            if (!User.Identity.IsAuthenticated && Project.GetProjectById(projectId).AccessType == Globals.ProjectAccessType.Private)
                Response.Redirect("~/Errors/AccessDenied.aspx", true);
            else if(User.Identity.IsAuthenticated && Project.GetProjectById(projectId).AccessType == Globals.ProjectAccessType.Private && !Project.IsUserProjectMember(User.Identity.Name, projectId))
                Response.Redirect("~/Errors/AccessDenied.aspx", true);

            projectName = Project.GetProjectById(projectId).Name;

            channel = new RssChannel();
            feed = new RssFeed();

            switch (channelId)
            {
                case 1:
                    MilestoneFeed();
                    break;
                case 2:
                    CategoryFeed();
                    break;
                case 3:
                    StatusFeed();
                    break;
                case 4:
                    PriorityFeed();
                    break;
                case 5:
                    TypeFeed();
                    break;
                case 6:
                    AssigneeFeed();
                    break;
                case 7:
                    FilteredIssuesFeed();
                    break;
                case 8:
                    RelevantFeed();
                    break;
                case 9:
                    AssignedFeed();
                    break;
                case 10:
                    OwnedFeed();
                    break;
                case 11:
                    CreatedFeed();
                    break;
                case 12:
                    AllFeed();
                    break;
                case 13:
                    QueryFeed();
                    break;
            }

            channel.PubDate = DateTime.Now;
            channel.LastBuildDate = channel.Items.LatestPubDate();
            channel.Link = new System.Uri(HostSetting.GetHostSetting("DefaultUrl") + "Rss.aspx?" + Request.QueryString.ToString());

            try
            {
                feed.Channels.Add(channel);
                feed.Encoding = System.Text.Encoding.UTF8;
                Response.ContentType = "text/xml";
                feed.Write(Response.OutputStream);
                Response.End();
            }
            catch
            {
                //exception
            }
        }

        #region QueryString Properties
        public string QueryId
        {
            get
            {
                if (Context.Request.Params["q"] == null)
                    return string.Empty;

                return Context.Request.Params["q"];
            }
        }
        ///<summary>
         ///Returns the component Id from the querystring
         ///</summary>
        public string IssueCategoryId
        {
            get
            {
                if (Context.Request.Params["c"] == null)
                {
                    return string.Empty;
                }
                return Context.Request.Params["c"];
            }
        }
        /// <summary>
        /// Returns the keywords from the querystring
        /// </summary>
        public string Key
        {
            get
            {
                if (Context.Request.Params["key"] == null)
                {
                    return string.Empty;
                }
                return Context.Request.Params["key"].Replace("+", " ");
            }
        }
        /// <summary>
        /// Returns the Milestone Id from the querystring
        /// </summary>
        public string IssueMilestoneId
        {
            get
            {
                if (Context.Request.Params["m"] == null)
                {
                    return string.Empty;
                }
                return Context.Request.Params["m"].ToString();
            }
        }


        /// <summary>
        /// Returns the priority Id from the querystring
        /// </summary>
        public string IssuePriorityId
        {
            get
            {
                if (Context.Request.Params["p"] == null)
                {
                    return string.Empty;
                }
                return Context.Request.Params["p"].ToString();
            }
        }
        /// <summary>
        /// Returns the Type Id from the querystring
        /// </summary>
        public string IssueTypeId
        {
            get
            {
                if (Context.Request.Params["t"] == null)
                {
                    return string.Empty;
                }
                return Context.Request.Params["t"].ToString();
            }
        }
        /// <summary>
        /// Returns the status Id from the querystring
        /// </summary>
        public string IssueStatusId
        {
            get
            {
                if (Context.Request.Params["s"] == null)
                {
                    return string.Empty;
                }
                return Context.Request.Params["s"].ToString();
            }
        }
        /// <summary>
        /// Returns the assigned to user Id from the querystring
        /// </summary>
        public string AssignedUserName
        {
            get
            {
                if (Request.Params["u"] == null)
                {
                    return string.Empty;
                }
                return Request.Params["u"].ToString();
            }
        }

        /// <summary>
        /// Gets the name of the reporter user.
        /// </summary>
        /// <value>The name of the reporter user.</value>
        public string ReporterUserName
        {
            get
            {
                if (Context.Request.Params["ru"] == null)
                {
                    return string.Empty;
                }
                return Context.Request.Params["ru"].ToString();
            }
        }
        /// <summary>
        /// Returns the hardware Id from the querystring
        /// </summary>
        public string IssueResolutionId
        {
            get
            {
                if (Context.Request.Params["r"] == null)
                {
                    return string.Empty;
                }
                return Context.Request.Params["r"].ToString();
            }
        }

        /// <summary>
        /// Gets the bug id.
        /// </summary>
        /// <value>The bug id.</value>
        public int IssueId
        {
            get { return Convert.ToInt32(Context.Request.Params["bid"]); }
        }
        #endregion


        /// <summary>
        /// Queries the feed.
        /// </summary>
        private void QueryFeed()
        {
            colIssues = Issue.PerformSavedQuery(projectId, Convert.ToInt32(QueryId));
           
            foreach (Issue issue in colIssues)
            {
                RssItem item = new RssItem();
                item.Title = string.Format("{0} - {1}", issue.FullId, issue.Title);
                item.Description = issue.Description;
                item.Author = issue.CreatorUserName;
                item.PubDate = issue.DateCreated;
                item.Link = new System.Uri(string.Format("{0}/Issues/IssueDetail.aspx?id={1}", HostSetting.GetHostSetting("DefaultUrl"), issue.Id.ToString()));
                channel.Items.Add(item);
            }

            channel.Title = projectName + " - Issues";
            if (!string.IsNullOrEmpty(User.Identity.Name))
            {
                channel.Title += " (generated for " + Security.GetDisplayName() + ")";
            }

            channel.Description = string.Format("Saved Query for {0}", projectName);
        }
        /// <summary>
        /// Alls the feed.
        /// </summary>
        private void AllFeed()
        {
            colIssues = Issue.GetIssuesByProjectId(projectId);

            foreach (Issue issue in colIssues)
            {
                RssItem item = new RssItem();
                item.Title = string.Format("{0} - {1}", issue.FullId, issue.Title);
                item.Description = issue.Description;
                item.Author = issue.CreatorUserName;
                item.PubDate = issue.DateCreated;
                item.Link = new System.Uri(string.Format("{0}/Issues/IssueDetail.aspx?id={1}", HostSetting.GetHostSetting("DefaultUrl"), issue.Id.ToString()));
                channel.Items.Add(item);
            }

            channel.Title = projectName + " - Issues";
            if (!string.IsNullOrEmpty(User.Identity.Name))
            {
                channel.Title += " (generated for " + Security.GetDisplayName() + ")";
            }

            channel.Description = string.Format("All Issues for {0}", projectName);
        }

        /// <summary>
        /// Createds the feed.
        /// </summary>
        private void CreatedFeed()
        {
            colIssues = Issue.GetIssuesByCreatorUserName(projectId, User.Identity.Name);

            foreach (Issue issue in colIssues)
            {
                RssItem item = new RssItem();
                item.Title = string.Format("{0} - {1}", issue.FullId, issue.Title);
                item.Description = issue.Description;
                item.Author = issue.CreatorUserName;
                item.PubDate = issue.DateCreated;
                item.Link = new System.Uri(string.Format("{0}/Issues/IssueDetail.aspx?id={1}", HostSetting.GetHostSetting("DefaultUrl"), issue.Id.ToString()));
                channel.Items.Add(item);
            }

            channel.Title = projectName + " - Issues";
            if (!string.IsNullOrEmpty(User.Identity.Name))
            {
                channel.Title += " (generated for " + Security.GetDisplayName() + ")";
            }

            channel.Description = string.Format("Created Issues for {0}", projectName);
        }

        /// <summary>
        /// Owneds the feed.
        /// </summary>
        private void OwnedFeed()
        {
            colIssues = Issue.GetIssuesByOwnerUserName(projectId, User.Identity.Name);

            foreach (Issue issue in colIssues)
            {
                RssItem item = new RssItem();
                item.Title = string.Format("{0} - {1}", issue.FullId, issue.Title);
                item.Description = issue.Description;
                item.Author = issue.CreatorUserName;
                item.PubDate = issue.DateCreated;
                item.Link = new System.Uri(string.Format("{0}/Issues/IssueDetail.aspx?id={1}", HostSetting.GetHostSetting("DefaultUrl"), issue.Id.ToString()));
                channel.Items.Add(item);
            }

            channel.Title = projectName + " - Issues";
            if (!string.IsNullOrEmpty(User.Identity.Name))
            {
                channel.Title += " (generated for " + Security.GetDisplayName() + ")";
            }

            channel.Description = string.Format("Owned Issues for {0}", projectName);
        }


        /// <summary>
        /// Assigneds the feed.
        /// </summary>
        private void AssignedFeed()
        {
            colIssues = Issue.GetIssuesByAssignedUserName(projectId, User.Identity.Name);

            foreach (Issue issue in colIssues)
            {
                RssItem item = new RssItem();
                item.Title = string.Format("{0} - {1}", issue.FullId, issue.Title);
                item.Description = issue.Description;
                item.Author = issue.CreatorUserName;
                item.PubDate = issue.DateCreated;
                item.Link = new System.Uri(string.Format("{0}/Issues/IssueDetail.aspx?id={1}", HostSetting.GetHostSetting("DefaultUrl"), issue.Id.ToString()));
                channel.Items.Add(item);
            }

            channel.Title = projectName + " - Issues";
            if (!string.IsNullOrEmpty(User.Identity.Name))
            {
                channel.Title += " (generated for " + Security.GetDisplayName() + ")";
            }

            channel.Description = string.Format("Assigned Issues for {0}", projectName);
        }

        /// <summary>
        /// Relevants the feed.
        /// </summary>
        private void RelevantFeed()
        {
            colIssues = Issue.GetIssuesByRelevancy(projectId, User.Identity.Name);

            foreach (Issue issue in colIssues)
            {
                RssItem item = new RssItem();
                item.Title = string.Format("{0} - {1}", issue.FullId, issue.Title);
                item.Description = issue.Description;
                item.Author = issue.CreatorUserName;
                item.PubDate = issue.DateCreated;
                item.Link = new System.Uri(string.Format("{0}/Issues/IssueDetail.aspx?id={1}", HostSetting.GetHostSetting("DefaultUrl"), issue.Id.ToString()));
                channel.Items.Add(item);
            }

            channel.Title = projectName + " - Issues";
            if (!string.IsNullOrEmpty(User.Identity.Name))
            {
                channel.Title += " (generated for " + Security.GetDisplayName() + ")";
            }

            channel.Description = string.Format("Relevant Issues for {0}", projectName);
        }

        /// <summary>
        /// Creates an RSS news feed for Issues By Version
        /// </summary>
        private void MilestoneFeed()
        {
            List<IssueCount> lsVersion = Issue.GetIssueMilestoneCountByProject(projectId);

            foreach (IssueCount issueCount in lsVersion)
            {
                RssItem item = new RssItem();
                item.Title = issueCount.Name;
                item.Description = string.Concat(issueCount.Count.ToString(), " Open Issues");
                item.PubDate = DateTime.Now;
                item.Link = new System.Uri(Page.ResolveUrl(string.Format("{2}/Issues/IssuesList.aspx?pid={0}&s=0&m={1}", projectId, issueCount.Id, HostSetting.GetHostSetting("DefaultUrl"))));
                channel.Items.Add(item);
            }

            channel.Title = projectName + " Issues By Milestone";
            channel.Description = "A listing of all " + projectName + " issues grouped by milestone";
        }

        /// <summary>
        /// Creates an RSS news feed for Issues By category
        /// </summary>
        private void CategoryFeed()
        {
            CategoryTree objComps = new CategoryTree();
            List<Category> al = objComps.GetCategoryTreeByProjectId(projectId);

            foreach (Category c in al)
            {
                RssItem item = new RssItem();
                item.Title = c.Name;
                item.Description = string.Concat(Issue.GetIssueCountByProjectAndCategory(projectId, c.Id).ToString(), " Open Issues");
                item.PubDate = DateTime.Now;
                item.Link = new System.Uri(Page.ResolveUrl(string.Format("{2}/Issues/IssuesList.aspx?pid={0}&s=0&c={1}", projectId, c.Id, HostSetting.GetHostSetting("DefaultUrl"))));
                channel.Items.Add(item);
            }

            channel.Title = projectName + " Issues By Component";
            channel.Description = "A listing of all " + projectName + " issues grouped by component";
        }

        /// <summary>
        /// Creates an RSS news feed for Issues By Status
        /// </summary>
        private void StatusFeed()
        {
            List<IssueCount> al = Issue.GetIssueStatusCountByProject(projectId);

            foreach (IssueCount issueCount in al)
            {
                RssItem item = new RssItem();
                item.Title = issueCount.Name;
                item.Description = string.Concat(issueCount.Count, " Issues");
                item.PubDate = DateTime.Now;
                item.Link = new System.Uri(Page.ResolveUrl(string.Format("{2}/Issues/IssuesList.aspx?pid={0}&s={1}", projectId, issueCount.Id, HostSetting.GetHostSetting("DefaultUrl"))));
                channel.Items.Add(item);
            }

            channel.Title = projectName + " Issues By Status";
            channel.Description = "A listing of all " + projectName + " issues grouped by status";
        }

        /// <summary>
        /// Creates an RSS news feed for Issues By Priority
        /// </summary>
        private void PriorityFeed()
        {
            List<IssueCount> al = Issue.GetIssuePriorityCountByProject(projectId);

            foreach (IssueCount issueCount in al)
            {
                RssItem item = new RssItem();
                item.Title = issueCount.Name;
                item.Description = string.Concat(issueCount.Count, " Open Issues");
                item.PubDate = DateTime.Now;
                item.Link = new System.Uri(Page.ResolveUrl(string.Format("{2}/Issues/IssuesList.aspx?pid={0}&s=0&p={1}", projectId, issueCount.Id, HostSetting.GetHostSetting("DefaultUrl"))));
                channel.Items.Add(item);
            }

            channel.Title = projectName + " Open Issues By Prioirty";
            channel.Description = "A listing of all " + projectName + " issues grouped by priority";
        }

        /// <summary>
        /// Creates an RSS news feed for Issues By Type
        /// </summary>
        private void TypeFeed()
        {
            List<IssueCount> al = Issue.GetIssueTypeCountByProject(projectId);

            foreach (IssueCount issueCount in al)
            {
                RssItem item = new RssItem();
                item.Title = issueCount.Name;
                item.Description = string.Concat(issueCount.Count, " Open Issues");
                item.PubDate = DateTime.Now;
                item.Link = new System.Uri(Page.ResolveUrl(string.Format("{2}/Issues/IssuesList.aspx?pid={0}&s=0&t={1}", projectId, issueCount.Id, HostSetting.GetHostSetting("DefaultUrl"))));
                channel.Items.Add(item);
            }

            channel.Title = projectName + " Open Issues By Type";
            channel.Description = "A listing of all " + projectName + " issues grouped by type";
        }

        /// <summary>
        /// Creates an RSS news feed for Issues By Assignee
        /// </summary>
        private void AssigneeFeed()
        {
            List<IssueCount> al = Issue.GetIssueUserCountByProject(projectId);

            foreach (IssueCount issueCount in al)
            {
                RssItem item = new RssItem();
                item.Title = issueCount.Name;
                item.Description = string.Concat(issueCount.Count, " Open Issues");
                item.PubDate = DateTime.Now;
                item.Link = new System.Uri(Page.ResolveUrl(string.Format("{2}/Issues/IssuesList.aspx?pid={0}&s=0&u={1}", projectId, issueCount.Id, HostSetting.GetHostSetting("DefaultUrl"))));
                channel.Items.Add(item);
            }


            RssItem uitem = new RssItem();
            uitem.Title = "Unassigned";
            uitem.Description = string.Concat(Issue.GetIssueUnassignedCountByProject(projectId).ToString(), " Open Issues");
            uitem.PubDate = DateTime.Now;
            uitem.Link = new System.Uri(Page.ResolveUrl(string.Format("{1}/Issues/IssuesList.aspx?pid={0}&s=0&u=0", projectId, HostSetting.GetHostSetting("DefaultUrl"))));
            channel.Items.Add(uitem);

            channel.Title = projectName + " Open Issues By Assignee";
            channel.Description = "A listing of all " + projectName + " issues grouped by assignee";
        }

        private void FilteredIssuesFeed()
        {
            QueryClause q;
            bool isStatus = false;
            string BooleanOperator = "AND";
            List<QueryClause> queryClauses = new List<QueryClause>();
            //if (!string.IsNullOrEmpty(IssueCategoryId))
            //{
            //    q = new QueryClause(BooleanOperator, "IssueCategoryId", "=", IssueCategoryId.ToString(), SqlDbType.Int, false);
            //    queryClauses.Add(q);
            //}
            if (!string.IsNullOrEmpty(IssueTypeId))
            {
                q = new QueryClause(BooleanOperator, "IssueTypeId", "=", IssueTypeId.ToString(), SqlDbType.Int, false);
                queryClauses.Add(q);
            }
            if (!string.IsNullOrEmpty(IssueMilestoneId))
            {
                //if zero, do a null comparison.
                if (IssueMilestoneId == "0")
                {
                    q = new QueryClause(BooleanOperator, "IssueMilestoneId", "IS", null, SqlDbType.Int, false);
                }
                else
                {
                    q = new QueryClause(BooleanOperator, "IssueMilestoneId", "=", IssueMilestoneId, SqlDbType.Int, false);
                }
                queryClauses.Add(q);
            }
            if (!string.IsNullOrEmpty(IssueResolutionId))
            {
                q = new QueryClause(BooleanOperator, "IssueResolutionId", "=", IssueResolutionId.ToString(), SqlDbType.Int, false);
                queryClauses.Add(q);
            }
            if (!string.IsNullOrEmpty(IssuePriorityId))
            {
                q = new QueryClause(BooleanOperator, "IssuePriorityId", "=", IssuePriorityId.ToString(), SqlDbType.Int, false);
                queryClauses.Add(q);
            }
            if (!string.IsNullOrEmpty(IssueStatusId))
            {
                isStatus = true;
                q = new QueryClause(BooleanOperator, "IssueStatusId", "=", IssueStatusId.ToString(), SqlDbType.Int, false);
                queryClauses.Add(q);
            }
            if (!string.IsNullOrEmpty(AssignedUserName))
            {
                q = new QueryClause(BooleanOperator, "IssueAssignedUserId", "=", AssignedUserName, SqlDbType.NVarChar, false);
                queryClauses.Add(q);
            }

            //exclude all closed status's
            if (!isStatus)
            {
                List<Status> status = Status.GetStatusByProjectId(projectId).FindAll(delegate(Status s) { return s.IsClosedState == true; });
                foreach (Status st in status)
                {
                    q = new QueryClause(BooleanOperator, "IssueStatusId", "<>", st.Id.ToString(), SqlDbType.Int, false);
                    queryClauses.Add(q);
                }
            }
            //q = new QueryClause(BooleanOperator, "new", "=", "another one", SqlDbType.NVarChar, true);
            //queryClauses.Add(q);
            colIssues = Issue.PerformQuery(projectId, queryClauses);

            foreach (Issue issue in colIssues)
            {
                RssItem item = new RssItem();
                item.Title = string.Format("{0} - {1}", issue.FullId, issue.Title);
                item.Description = issue.Description;
                item.Author = issue.CreatorUserName;
                item.PubDate = issue.DateCreated;
                item.Link = new System.Uri(string.Format("{0}/Issues/IssueDetail.aspx?id={1}", HostSetting.GetHostSetting("DefaultUrl"), issue.Id.ToString()));
                channel.Items.Add(item);
            }
            
            channel.Title = projectName + " - Issues";
            if (!string.IsNullOrEmpty(User.Identity.Name)) {
               channel.Title += " (generated for " + Security.GetDisplayName() + ")";
            }
           
            channel.Description = string.Format("Issues for {0}",projectName);

        }

	
    }
}
