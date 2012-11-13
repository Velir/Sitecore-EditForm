using Sitecore.Data.Items;

namespace Sitecore.SharedSource.EditForm.CustomItems.Common.Editform
{
	public class EditFormButtonItem : CustomItem
	{
		public static string TemplateId = "{09980A14-FEDA-4987-A86D-7BAAFA02738A}";

		#region Boilerplate CustomItem Code

		public EditFormButtonItem(Item innerItem)
			: base(innerItem)
		{
		}

		public static implicit operator EditFormButtonItem(Item innerItem)
		{
			return innerItem != null ? new EditFormButtonItem(innerItem) : null;
		}

		public static implicit operator Item(EditFormButtonItem customItem)
		{
			return customItem != null ? customItem.InnerItem : null;
		}

		#endregion //Boilerplate CustomItem Code

		#region Field Instance Methods

		public string Title
		{
			get { return InnerItem["Title"]; }
		}

		public string Description
		{
			get { return InnerItem["Description"]; }
		}

		public string Click
		{
			get { return InnerItem["Click"]; }
		}

		public string ImagePath
		{
			get { return InnerItem["Image Path"]; }
		}

		#endregion
	}
}