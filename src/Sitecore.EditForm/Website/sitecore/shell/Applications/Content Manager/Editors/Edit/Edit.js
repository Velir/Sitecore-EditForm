Sitecore.Edit = new function() {
	Sitecore.registerClass(this, "Sitecore.Edit");
	Sitecore.UI.ModifiedTracking.track(true);
	Sitecore.Dhtml.attachEvent(window, "onload", function() { Sitecore.Edit.load() } );
}

Sitecore.Edit.load = function () {

	//get children elements to wire click event
	$$('.scTileItem').invoke('observe', 'click', ItemClickHandler);
}

Sitecore.Edit.CopyItems = function (currentItemId) {
	var selectedValues = GetSelectedItems();
	if(selectedValues.length == 0){
		alert('Please select items');
		return;
	}

	var queryString = FormatForQueryString(selectedValues);
	if(queryString == ''){
		return;
	}

	//launch copy to form
	scForm.getParentForm().invoke('editform:copymultipleto(id=' + currentItemId + ',selected=' + queryString + ')');
}

Sitecore.Edit.CutItems = function (currentItemId) {
	var selectedValues = GetSelectedItems();
	if(selectedValues.length == 0){
		alert('Please select items');
		return;
	}

	var queryString = FormatForQueryString(selectedValues);
	if(queryString == ''){
		return;
	}

	//launch copy to form
	scForm.getParentForm().invoke('editform:cutmultipleto(id=' + currentItemId + ',selected=' + queryString + ')');
}

Sitecore.Edit.DeleteItems = function (currentItemId) {
	var selectedValues = GetSelectedItems();
	if (selectedValues.length == 0) {
		alert('Please select items');
		return;
	}

	var queryString = FormatForQueryString(selectedValues);
	if (queryString == '') {
		return;
	}

	//launch copy to form
	scForm.getParentForm().invoke('editform:deletemultiple(id=' + currentItemId + ',selected=' + queryString + ')');
}

//Handles the Click Event for the Item's Children
function ItemClickHandler(event) {
	var element = GetProperElement(event.element());

	if (hasClass(element, "selected")) {
		removeClass(element, "selected");
	}
	else {
		addClass(element, "selected");
	}
}

//Returns a properly formatted querystring value
function FormatForQueryString(selectedItems){
	var queryString = '';
	var i = 0;
	while(i < selectedItems.length)
	{
		//add delimiter
		if(queryString != ''){
			queryString += '|';
		}

		//concatenate id
		queryString += selectedItems[i];
		i++;
	}
	return queryString;
}

//Retrieves the selected items from the form
function GetSelectedItems() {

	var selectedValues = new Array();

	var i = 0;
	$$('a.scEditTileItem.selected').each(function (item) {
		selectedValues[i] = $(item).readAttribute('data_id');
		i = i + 1;
	});

	return selectedValues;
}

function ToggleEdit(e) {
	
	var frame = scForm.browser.getFrameElement(window);
	
	if (request.parameters == "contenteditor:save") {
		window.location.reload(false);
	}

	return null;
}

function GetProperElement(element) {

	//check tag
	if (hasClass(element, "scEditTileItem")) {
		return element;
	}

	//walk up dom
	var i = 0;
	var currentElement = element;
	while(i <= 6)
	{
		if (hasClass(currentElement, "scEditTileItem")) {
			return currentElement;
		}

		if (currentElement.parentNode == null) {
			return null;
		}

		currentElement = currentElement.parentNode;
		i++;
	}

	return null;
}

function hasClass(element, className) {
	if (element == null) {
		return false;
	}

	var elementClassName = element.className;
	return (elementClassName.length > 0 && (elementClassName == className || new RegExp("(^|\\s)" + className + "(\\s|$)").test(elementClassName)));
}

function addClass(element, className) {

	if (element == null) {
		return false;
	}

	if (!hasClass(element, className))
	{
		element.className += (element.className ? ' ' : '') + className;
	}
}

function removeClass(element, className) {
	if (element == null) {
		return false;
	}

	element.className = element.className.replace(new RegExp("(^|\\s+)" + className + "(\\s+|$)"), ' ').strip();
}