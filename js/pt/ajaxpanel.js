//Ajax Panel @0-90B3255F
window.AjaxPanel = AjaxPanel = {
	init: function(panel) {
		if (panel.filterId == null) {
			panel.filterId = panel.id;
		}
		if (panel.action == null) {
			panel.action = $$("form")[0].action;
		}
	},

	reload: function(panel) {
		var requestUrl = window.location.toString();
		var hashIndex = requestUrl.indexOf("#");
		if (hashIndex != -1) {
			requestUrl = requestUrl.substring(hashIndex + 1, requestUrl.length);
		}
		AjaxPanel._showProgress();
		new Ajax.Request(AjaxPanel._addParam(requestUrl,"FormFilter",panel.filterId), {
			method: 'get',
			requestHeaders: ['If-Modified-Since', new Date(0)],
			onSuccess: function(transport) {
				AjaxPanel._update(panel, transport.responseText);
				AjaxPanel._hideProgress();
			}
		});
	},

	_bind: function(panel) {
		var links = $$("#" + panel.id + " a");
		var inputs = $$("#" + panel.id + " input");
		var selects = $$("#" + panel.id + " select");
		var form = $$("form")[0];
		links.each(function(link) {
			if ( AjaxPanel._matchPages(document.location.href, link.href) && (link.target == "" || link.target.toLowerCase() == "_self") ) {
				link.panel = panel;
				link.onclick = function() { return false; }
				link.observe("click", AjaxPanel._linkClick);
				// link.href = link.href.replace("FormFilter=" + panel.id,"");
				if (link.href.lastIndexOf("&") == link.href.length - 1) link.href = link.href.substring(0,link.href.length - 1);
			}
			if (link.href.indexOf("javascript:__doPostBack") == 0) {
				link.href = link.href.replace("javascript:__doPostBack", "javascript:__doAjaxPanelPostBack");
				link.panel = panel;
			}
		});
		window.ccs_ap_f = function() {
			var existingSubmit = form.onsubmit ? form.onsubmit : function() { };
			form.onsubmit = function() {
				if (existingSubmit.apply(form) != false) {
					try {
						if(typeof(FCKeditorAPI) == "object") {
							var textareas = this.getElementsByTagName("textarea");
							for (var t = 0; t < textareas.length; t++) {
								var textareaId = textareas[t].getAttribute("id");
								if (textareaId && textareaId != "")
								{
									var fckInstance = FCKeditorAPI.GetInstance(textareaId);
									if (fckInstance) fckInstance.UpdateLinkedField();
								}
							}
						}
					}
					catch(err) { }
					return AjaxPanel._submitForm(form);
					//         this.submitted = true;
				}
				return false;
			};
		};
		inputs.each(function(input) {
			if (input.form != null) {
				if (input.type != null && (input.type.toLowerCase() == "submit" || input.type.toLowerCase() == "image")) {
					//TODO: Also catch submitting by Enter
					input.panel = panel;
					input.observe("click", AjaxPanel._inputClick);
				}
			}
		});
		selects.each(function(select) {
			// Check if Navigator has PageSize Selector
			if (select.onchange != null && select.name.indexOf("Navigator$") != -1) {
				select.onchange = null;
				select.panel = panel;
				select.observe("change", AjaxPanel._selectChange);
			}
		});        
		window.setTimeout("window.ccs_ap_f()",0);
		window.setTimeout("try { delete window.ccs_ap_f; } catch (e) { window.ccs_ap_f = null; }", 1000);
		if(panel.onrefresh) {
			panel.onrefresh.apply(panel);
		}
	},

	_getPostData: function(form) {
		if (form.method.toLowerCase() == "post") {
			var postQuery = "";
			for (var i=0;i<form.elements.length;i++) {
				var element = form.elements[i];
				if (element.name != null) {
					if ( element.type != null && (element.type.toLowerCase() == "checkbox" || element.type.toLowerCase() == "radio") ) {
						if (element.checked) { postQuery += element.name + "=" + escape(element.value) + "&"; }
					} else if (element.type == null || (element.type != null && element.type != "submit" && element.type != "image")) {
                        			postQuery += element.name + "=" + encodeURIComponent(element.value).replace(/\//g,"%2F").replace(/\+/g,"%2B") + "&";
					}
				}
			}
			return postQuery;
		}
        	return undefined; //TODO: maybe null?
	},
	
	_inputClick: function(event) {
		var sender = Event.element(event);
		sender.form.lastClick = sender;
	},

	_selectChange: function(event) {
		var sender = Event.element(event);
		__doAjaxPanelPostBack(sender.name,'');
	},

	_submitForm: function(form) {
		if (form != null) {
			var sender = form.lastClick;
			if (!sender || !sender.form)
				return true;
			//TODO: Support for empty action and action w/o params
			AjaxPanel._showProgress();
			var postParams = AjaxPanel._getPostData(sender.form);
			if (sender.type && sender.type.toLowerCase() == "submit") {
				postParams += sender.name + "=" + sender.value;
			}
			if (sender.type && sender.type.toLowerCase() == "image") {
				postParams += sender.name + ".x=1&" + sender.name + ".y=1";
			}
			new Ajax.Request(AjaxPanel._addParam(sender.panel.action,"FormFilter",sender.panel.filterId), {
				method: sender.form.method,
				requestHeaders: ['If-Modified-Since', new Date(0)],
				postBody: postParams,
				onSuccess: function(transport) {
					AjaxPanel._update(sender.panel, transport.responseText);
					AjaxPanel._hideProgress();
				}
			});
		}
		return false;
	},

	_linkClick: function(event) {
		var sender = Event.element(event);
		while (!sender.panel) sender = sender.parentNode;
		var appendSymbol = "&";
		var hashIndex = sender.href.indexOf("#");
		if (hashIndex != -1) {
			sender.href = sender.href.substring(0, hashIndex);
		}
		AjaxPanel._showProgress();
		var url = AjaxPanel._addParam(sender.href,"FormFilter",sender.panel.filterId);
		new Ajax.Request(url, {
			method: "get",
			requestHeaders: ['If-Modified-Since', new Date(0)],
			onSuccess: function(transport) {
				AjaxPanel._update(sender.panel, transport.responseText);
				AjaxPanel._hideProgress();
			}
		});
		return false;
	},

	_update: function(panel, content) {
		// IE removes a <script> tag if it goes first, so we should add something before it
		if ((new RegExp("^<script", "m")).test(content)) content = "<span style='display:none;'>&nbsp;</span>" + content;
		$(panel.id).innerHTML = AjaxPanel._prepareAspNetHiddenFields(panel,content);
		// if there is a javascript inside a HTML comment IE will not exec this script
		var re1 = /^\s*<!--/m, re2 = /-->\s*$/m;
		var scrs = $(panel.id).getElementsByTagName('script');
		for(var i = 0; i < scrs.length; i++){
			eval(scrs[i].innerHTML.replace(re1, "").replace(re2, ""));
		}
		AjaxPanel._bind(panel);
		try {
			setTimeout(panel.id + "_bind()", 0);
		} catch(e) {};
	},

	_matchPages: function(pageUrl, linkUrl) {
		if (linkUrl == null || linkUrl == "#" || linkUrl == "") { return false; }
		if (AjaxPanel._getScriptPath(pageUrl) == AjaxPanel._getScriptPath(linkUrl)) {
			return true;
		}
		return false;
	},

	_getScriptPath: function(fullUrl) {
		var questPos = fullUrl.indexOf("?");
		if (questPos == -1) { return fullUrl };
		return fullUrl.substring(0, questPos);
	},

    _showProgress: function() {
        if ($("AjaxPanelProgress") != null) {
            return false;
        }
        var progressSpan = document.createElement("div");
        progressSpan.style.position = "absolute";
        progressSpan.style.zIndex = 1000;
        progressSpan.style.top = "3px";
        progressSpan.style.right = "3px";
        progressSpan.id = "AjaxPanelProgress";
        progressSpan.innerHTML = "<div style=\"background-color: #D33333; font-family: Tahoma; font-size: 8pt; padding:1px; color: #FFFFFF;\">&nbsp;Loading...&nbsp;</div>";
        document.body.appendChild(progressSpan);
    },

	_hideProgress: function() {
		if ($("AjaxPanelProgress") == null) {
			return false;
		}
		document.body.removeChild($("AjaxPanelProgress"));
	},
    
	_addFormFilter: function(qString, fId) {
		if (qString.indexOf("FormFilter=" + fId) == -1) {
			if (qString.indexOf("?") != -1) {
				qString += "&FormFilter=" + fId
			} else {
				qString += "?FormFilter=" + fId
			} 
		}
		return qString;
	},
    
	_prepareAspNetHiddenFields: function(panel,content) {
		tagsRe = /<vs=\"([^\"]*)\"\/><ev=\"([^\"]*)\"\/><fa=\"([^\"]*)\"\/>/g
		panelContent = content.replace(tagsRe, "");
		tags = content.substring(panelContent.length);
		$("__VIEWSTATE").value = tags.replace(tagsRe,"$1");
		panel.action = encodeURI(unescape(tags.replace(tagsRe,"$3")));
		if ($("__EVENTVALIDATION") != null)
			$("__EVENTVALIDATION").value = tags.replace(tagsRe,"$2");
		return panelContent;
	},

    _addParam: function(queryStr, paramName, paramValue) {
        queryStr = decodeURI(queryStr);
        queryStr = queryStr.replace(/\+/g, " ");
        var queryParts = queryStr.split("?");
        var params = new Hash();
        if (queryParts.length > 1) {
            params = new Hash(queryStr.toQueryParams());
        }
        params.set(paramName, paramValue);
        return queryParts[0] + "?" + params.toQueryString();
    },

    _removeParam: function(queryStr, paramName) {
        var queryParts = queryStr.split("?");
        var params = new Hash();
        if (queryParts.length > 1) {
            params = new Hash(queryStr.toQueryParams());
            params.unset(paramName);
        }
        return queryParts[0] + "?" + params.toQueryString();
    }
}

function __doAjaxPanelPostBack(eventTarget, eventArgument) {
	var sender = $$("a").find(function(link) {
		return link.href.indexOf(eventTarget) != -1 
	});
	sender = (sender == undefined) ? $$("select").find(function(select) {
		return select.name == eventTarget 
	}) : sender;
	AjaxPanel._showProgress();
	$("__EVENTTARGET").value =  eventTarget; 
	$("__EVENTARGUMENT").value = eventArgument;	
	var postParams = AjaxPanel._getPostData($("ctl00"));
	new Ajax.Request(AjaxPanel._addParam(sender.panel.action,"FormFilter",sender.panel.filterId), {
		method: "post",
		postBody: postParams,
		onSuccess: function(transport) {
			AjaxPanel._update(sender.panel, transport.responseText);
			AjaxPanel._hideProgress();
		}
	});               
}                

//End Ajax Panel

