using System.Linq;
using Sitecore.Data.Items;
using Sitecore.SharedSource.Commons.Extensions;
using Sitecore.Shell.Framework;
using Sitecore.Shell.Framework.Commands;

namespace Sitecore.SharedSource.EditForm.CustomSitecore.Commands
{
	public class CopyMultipleToCommand : Command
	{
		/// <summary>
		/// Executes the command in the specified context.
		/// </summary>
		/// <param name="context">The context.</param>
		public override void Execute(CommandContext context)
		{
			if (context.Items.Length == 1)
			{
				//get selected items
				string selectedItems = context.Parameters["selected"];
				var items = selectedItems.Split('|').Select(id => Sitecore.Context.ContentDatabase.GetItem(id)).ToArray();

				//verify items still exist.
				Item[] itemsToCopy = items.Where(x => x.IsNotNull()).ToArray();

				//perform action
				Items.CopyTo(itemsToCopy);

				//refresh content manager
				string currentId = context.Parameters["id"];
				Sitecore.Context.ClientPage.SendMessage(this, string.Format("item:refresh(id={0})", currentId));
				Sitecore.Context.ClientPage.SendMessage(this, string.Format("item:refreshchildren(id={0})", currentId));
				Sitecore.Context.ClientPage.SendMessage(this, string.Format("item:load(id={0})", currentId));
			}
		}
	}
}