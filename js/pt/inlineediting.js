function InlineEditing(formName) {
	this.formName = formName;
	this.form = document.forms[formName];
	this.editableControls = this.getEditableControls();
	this.rowsNumber = this.getRowsNumber();
	this.editableRowsNumber = this.getEditableRowsNumber();
	this.submitButton = this.getSubmitButton();
	this.submitButton.hide();
	var _this = this;
	this.iterateEditableControls(function(rowNumber) {
		if (rowNumber <= _this.editableRowsNumber) {
			var elementTd = _this.hideTd(this);
			var newNode = document.createElement("span");
			newNode.innerHTML = this.value + "&nbsp;";
			elementTd.appendChild(newNode);
		} else {
			_this.hideTr(this);
		}
	});
	this.iterateCheckboxes(function() {
		var elementTd = _this.hideTd(this);
		var currentRow = _this.getRow(this);
		// Creating Edit
		var editLink = document.createElement("a");
		editLink.id = "IEdit_" + currentRow;
		editLink.innerHTML = "Edit";
		editLink.href = "#";
		editLink.row = currentRow;
		editLink.onclick = function() { return false; };
		editLink.formName = formName;
		elementTd.appendChild(editLink);
		Event.observe(editLink, "click", function (event) {
			var sender = Event.element(event);
			_this.editClick(sender);
		});
		elementTd.appendChild(document.createTextNode(" "));
		// Creating Update
		var updateLink = document.createElement("a");
		updateLink.id = "IUpdate_" + currentRow;
		updateLink.innerHTML = "Update";
		updateLink.href = "#";
		updateLink.row = currentRow;
		updateLink.onclick = function() { return false; };
		elementTd.appendChild(updateLink);
		Event.observe(updateLink, "click", function(event) {
			var sender = Event.element(event);
			_this.updateClick(sender);
		});
		elementTd.appendChild(document.createTextNode(" "));
		updateLink.hide();
		// Creating Delete
		var deleteLink = document.createElement("a");
		deleteLink.id = "IDelete_" + currentRow;
		deleteLink.innerHTML = "Delete";
		deleteLink.href = "#";
		deleteLink.row = currentRow;
		deleteLink.onclick = function() { return false; };
		deleteLink.formName = formName;
		elementTd.appendChild(deleteLink);
		Event.observe(deleteLink, "click", function(event) {
			var sender = Event.element(event);
			_this.deleteClick(sender);
		});
		deleteLink.hide();
	});
};

InlineEditing.prototype = {
	hideTd: function(element) {
		var elementTd = element.parentNode;
		if (elementTd.nodeName == "td" || elementTd.nodeName == "TD") {
			for (var j = 0; j < elementTd.childNodes.length; j++) {
				var childNode = elementTd.childNodes[j];
				if (childNode.hide) {
					childNode.hide();
				}
			}
		}
		return elementTd;
	},
	showTd: function(element) {
		var elementTd = element.parentNode;
		if (elementTd.nodeName == "td" || elementTd.nodeName == "TD") {
			for (var j = 0; j < elementTd.childNodes.length; j++) {
				var childNode = elementTd.childNodes[j];
				if (childNode.show) {
					childNode.show();
				}
			}
		}
		return elementTd;
	},
	getRow: function(element) {
		if (element.name.search(/([0-9]+)$/) != -1) {
			return RegExp.$1;
		}
		return 0;
	},
	editClick: function(sender) {
		var _this = this;
		this.iterateEditableControls(function() {
			var elementTd = _this.showTd(this);
			var lastChild = elementTd.lastChild;
			lastChild.style.display = "none";
		}, sender.row);
		$("IEdit_" + sender.row).hide();
		$("IUpdate_" + sender.row).show();
		$("IDelete_" + sender.row).show();
	},
	updateClick: function(sender) {
		AjaxPanel._submitForm(this.submitButton);
	},
	deleteClick: function(sender) {
		this.form["CheckBox_Delete_" + sender.row].checked = true;
		AjaxPanel._submitForm(this.submitButton);
	},
	insertClick: function(sender) {
		//this.iterateEditableControls()
	},
	iterateEditableControls: function(callbackFunc, rowNumber) {
		for (var i = 0; i < this.editableControls.length; i++) {
			var startRow = 1;
			var endRow = this.rowsNumber;
			if (rowNumber != null) {
				startRow = rowNumber;
				endRow = rowNumber;
			}
			for (var j = startRow; j <= endRow; j++) {
				var controlName = this.editableControls[i] + "_" + j;
				var element = this.form[controlName];
				callbackFunc.apply(element, [j]);
			}
		}
	},
	iterateCheckboxes: function(callbackFunc, formName) {
		for (var i = 1; i <= this.editableRowsNumber; i++) {
			var controlName = "CheckBox_Delete_" + i;
			var element = this.form[controlName];
			callbackFunc.call(element);
		}
	},
	getEditableControls: function() {
		var foundControls = [];
		for (var i = 0; i < this.form.elements.length; i++) {
			var element = this.form.elements[i];
			if (element.type && element.type == "text") {
				if (element.name.search(/^(.+)_1$/) != -1) {
					foundControls.push(RegExp.$1);
				}
			}
		}
		return foundControls;
	},
	getRowsNumber: function() {
		var rowsNumber = 0;
		for (var i = 0; i < this.form.elements.length; i++) {
			var element = this.form.elements[i];
			if (element.name.search(/_([0-9]+)$/) != -1) {
				if (Number(RegExp.$1) > rowsNumber) {
					rowsNumber = Number(RegExp.$1);
				}
			}
		}
		return rowsNumber;
	},
	getSubmitButton: function() {
		for (var i = 0; i < this.form.elements.length; i++) {
			var element = this.form.elements[i];
			if (element.type && element.type == "submit") {
				return element;
			}
		}
	},
	getEditableRowsNumber: function() {
		var formState = this.form.elements["FormState"].value;
		formStateParts = formState.split(";");
		return formStateParts[0];
	},
	hideTr: function(element) {
		var elementTr = element.parentNode.parentNode;
		if (elementTr.nodeName == "tr" || elementTr.nodeName == "TR") {
			elementTr.hide();
		}
		return elementTr;
	},
	showTr: function(element) {
		var elementTr = element.parentNode.parentNode;
		if (elementTr.nodeName == "tr" || elementTr.nodeName == "TR") {
			elementTr.show();
		}
		return elementTr;
	}
};