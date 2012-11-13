using System;
using System.Collections.Generic;
using System.IO;
using System.Web.UI;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Resources;
using Sitecore.SharedSource.Commons.Extensions;
using Sitecore.SharedSource.EditForm.CustomItems.Common.Editform;
using Sitecore.Shell;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web;
using Sitecore.Web.UI;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.XamlSharp.Xaml;
using Version = Sitecore.Data.Version;

namespace Velir.SitecoreLibrary.Modules.EditForm.CustomSitecore.Applications.Content_Editor.Editors
{
	public class EditForm : XamlMainControl, IHasCommandContext
	{
		// Fields
		private string _buttonsTitle = "Options";
		private string _itemClick = "item:load(id={0})";
		private string _itemsTitle = "Items in the Folder";
		private bool _renderInsertOptions = true;
		protected Scrollbox ItemList;

		protected virtual string GetClick(Item item)
		{
			Assert.ArgumentNotNull(item, "item");
			return ("javascript:scForm.getParentForm().invoke('" + string.Format(this.ItemClick, item.ID) + "');return false");
		}

		protected virtual List<Item> GetItems(Item item)
		{
			Assert.ArgumentNotNull(item, "item");
			List<Item> list = new List<Item>();
			list.AddRange(item.Children.ToArray());
			return list;
		}

		protected override void OnLoad(EventArgs e)
		{
			Assert.ArgumentNotNull(e, "e");
			base.OnLoad(e);
			if (!XamlControl.AjaxScriptManager.IsEvent)
			{
				this.Render();
			}
		}

		protected void Refresh_Click()
		{
			SheerResponse.SetLocation(string.Empty);
		}

		/// <summary>
		/// Render the Edit Form
		/// </summary>
		private void Render()
		{
			string queryString = WebUtil.GetQueryString("id");
			string name = WebUtil.GetQueryString("language");
			string databaseName = WebUtil.GetQueryString("database");
			ItemUri uri = new ItemUri(queryString, Language.Parse(name), Version.Latest, databaseName);
			Item item = Database.GetItem(uri);
			string pageText = string.Empty;

			if (item != null)
			{
				HtmlTextWriter output = new HtmlTextWriter(new StringWriter());
				this.RenderButtons(output, item);
				this.RenderItems(output, item);
				pageText = output.InnerWriter.ToString();
			}

			this.ItemList.InnerHtml = pageText;
		}

		/// <summary>
		/// Renders a Button
		/// </summary>
		/// <param name="innerOutput"></param>
		/// <param name="button"></param>
		protected virtual void RenderButton(HtmlTextWriter innerOutput, EditFormButtonItem button)
		{
			Assert.ArgumentNotNull(innerOutput, "innerOutput");
			Assert.ArgumentNotNull(button, "button");

			if (button == null || string.IsNullOrEmpty(button.ImagePath) || string.IsNullOrEmpty(button.Title))
			{
				return;
			}

			string clickEvent = button.Click;
			clickEvent = clickEvent.Replace("#id#", WebUtil.GetQueryString("id"));

			innerOutput.Write("<a href=\"#\" class=\"scButton\" title=\"" + button.Description + "\" onclick=\"" + clickEvent + "\">");
			innerOutput.Write("<img width=\"32\" border=\"0\" height=\"32\" alt=\"" + button.Title + "\" class=\"scButtonIconImage\" src=\"" + button.ImagePath + "\" />");
			innerOutput.Write("<div class=\"scButtonHeader\">" + Translate.Text(button.Title) + "</div>");
			innerOutput.Write("</a>");
		}

		/// <summary>
		/// Renders the Edit Forms Buttons
		/// </summary>
		/// <param name="output"></param>
		/// <param name="item"></param>
		protected virtual void RenderButtons(HtmlTextWriter output, Item item)
		{
			Assert.ArgumentNotNull(output, "output");
			Assert.ArgumentNotNull(item, "item");
			HtmlTextWriter writer = new HtmlTextWriter(new StringWriter());

			foreach(EditFormButtonItem button in GetEditFormButtons())
			{
				RenderButton(writer, button);
			}

			string str = writer.InnerWriter.ToString();
			if (!string.IsNullOrEmpty(str))
			{
				output.Write("<div class=\"scTitle scButtonsTitle\">");
				output.Write(Translate.Text(this.ButtonsTitle));
				output.Write("</div>");
				output.Write("<div class=\"scButtons\">");
				output.Write(str);
				output.Write("</div>");
			}
		}

		/// <summary>
		/// This will return the button items from Sitecore
		/// </summary>
		/// <returns></returns>
		protected virtual List<EditFormButtonItem> GetEditFormButtons()
		{
			//get database
			Database db = Database.GetDatabase("master");
			if(db == null)
			{
				return new List<EditFormButtonItem>();
			}

			Item moduleItem = Sitecore.Context.ContentDatabase.GetItem("/sitecore/system/Modules/Edit Form");
			if(moduleItem.IsNull())
			{
				return new List<EditFormButtonItem>();
			}

			//iterate over children
			List<EditFormButtonItem> buttons = new List<EditFormButtonItem>();
			foreach(Item childItem in moduleItem.Children)
			{
				if(childItem.IsNull() || !childItem.IsOfTemplate(EditFormButtonItem.TemplateId))
				{
					continue;
				}

				buttons.Add(childItem);
			}

			return buttons;
		}

		/// <summary>
		/// Renders an item onto the Edit Form
		/// </summary>
		/// <param name="output"></param>
		/// <param name="item"></param>
		protected virtual void RenderItem(HtmlTextWriter output, Item item)
		{
			Assert.ArgumentNotNull(output, "output");
			Assert.ArgumentNotNull(item, "item");

			string itemHtml = "<a class=\"scTileItem scEditTileItem\" " +
				"href=\"#\" " +
				string.Format("id=\"I{0}\" ", item.ID) +
				string.Format("data_id=\"{0}\">", item.ID) +

					"<table cellspacing=\"0\" cellpadding=\"0\" border=\"0\" class=\"scTileItemPane\">" +
						"<tbody>" +
							"<tr>" +
								"<td class=\"scTileItemIcon\">" +
									Images.GetImage(item.Appearance.Icon, ImageDimension.id32x32) +
								"</td>" +
								"<td class=\"scTileItemDetails\">" +
									string.Format("<div class=\"scTileItemDetailsHeader\">{0}</div>", item.DisplayName) +
								"</td>" +
							"</tr>" +
						"</tbody>" +
					"</table>" +
				"</a>";

			output.Write(itemHtml);
		}

		/// <summary>
		/// Renders the current items children onto the Edit Form
		/// </summary>
		/// <param name="output"></param>
		/// <param name="item"></param>
		protected virtual void RenderItems(HtmlTextWriter output, Item item)
		{
			Assert.ArgumentNotNull(output, "output");
			Assert.ArgumentNotNull(item, "item");
			List<Item> items = this.GetItems(item);
			if (items.Count != 0)
			{
				output.Write("<div class=\"scTitle scTilesTitle\">");
				output.Write(Translate.Text(this.ItemsTitle));
				output.Write("</div>");
				output.Write("<div class=\"scTiles\">");
				foreach (Item item2 in items)
				{
					if (UserOptions.View.ShowHiddenItems || !item2.Appearance.Hidden)
					{
						this.RenderItem(output, item2);
					}
				}
				output.Write("</div>");
			}
			else
			{
				output.Write("<div class=\"scEmpty\">" + Translate.Text("The folder is empty.") + "</div>");
			}
		}

		#region Sitecore Property Section
		protected string ButtonsTitle
		{
			get
			{
				return this._buttonsTitle;
			}
			set
			{
				Assert.ArgumentNotNull(value, "value");
				this._buttonsTitle = value;
			}
		}

		public string ContextMenuSourceID
		{
			get
			{
				return StringUtil.GetString(this.ViewState["ContextMenuSourceID"]);
			}
			set
			{
				Assert.ArgumentNotNull(value, "value");
				this.ViewState["ContextMenuSourceID"] = value;
			}
		}

		protected string ItemClick
		{
			get
			{
				return this._itemClick;
			}
			set
			{
				Assert.ArgumentNotNull(value, "value");
				this._itemClick = value;
			}
		}

		protected string ItemsTitle
		{
			get
			{
				return this._itemsTitle;
			}
			set
			{
				Assert.ArgumentNotNull(value, "value");
				this._itemsTitle = value;
			}
		}

		protected bool RenderInsertOptions
		{
			get
			{
				return this._renderInsertOptions;
			}
			set
			{
				this._renderInsertOptions = value;
			}
		}

		#endregion

		/// <summary>
		/// Gets the command context.
		/// </summary>
		/// <returns>
		/// The command context.
		/// </returns>
		public CommandContext GetCommandContext()
		{
			return new CommandContext();
		}
	}
}
