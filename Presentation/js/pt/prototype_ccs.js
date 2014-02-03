// CodeCharge extensions for prototype.

function addProgress() {
	if ($("AjaxPanelProgress") != null) {
		return false;
	}
	var progressSpan = document.createElement("span");
	progressSpan.style.position = "absolute";
	progressSpan.style.zIndex = 1000;
	progressSpan.style.top = 3;
	progressSpan.style.right = 3;
	progressSpan.id = "progress";
	progressSpan.innerHTML = "<div style=\"background-color: #D33333; font-family: Tahoma; font-size: 8pt; padding:1px; color: #FFFFFF;\">&nbsp;Loading...&nbsp;</div>";
	document.body.appendChild(progressSpan);
}
function removeProgress() {
	if ($("progress") == null) {
		return false;
	}
	document.body.removeChild($("progress"));
}

function addOpacity(transparent) {
	if ($("opacity") != null) {
		return false;
	}

/*
	var progressSpan = document.createElement("span");
	progressSpan.style.position = "absolute";
	progressSpan.style.zIndex = 1000;
	progressSpan.style.top = 3;
	progressSpan.style.right = 3;
	progressSpan.id = "opacity";
	progressSpan.innerHTML = "<div style=\"visibility: hidden; position: absolute; left: 0px; top: 0px; width:100%; height:100%; text-align:center; z-index: 2000; background-color: #000000; opacity: 0.40; filter: Alpha(opacity=40);   \">&nbsp;Loading...&nbsp;</div>";
	document.body.appendChild(progressSpan);
*/



	var progressSpan = document.createElement("div");

	progressSpan.style.position = "absolute";
	progressSpan.style.display = "block";
	progressSpan.style.zIndex = 50;
	progressSpan.style.top = 0;
	progressSpan.style.left = 0;
	//progressSpan.style.width = "100%";
	//progressSpan.style.height = "100%";

	var windowHeight = document.documentElement.scrollHeight ? document.documentElement.scrollHeight : document.documentElement.clientHeight ? document.documentElement.clientHeight : window.innerHeight;
    var windowWidth = "100%";
    //var windowWidth = document.documentElement.scrollWidth ? document.documentElement.scrollWidth : document.documentElement.clientWidth ? document.documentElement.clientWidth : window.innerWidth;

	progressSpan.style.width = windowWidth;
	progressSpan.style.height = windowHeight + "px";
	progressSpan.style.backgroundColor = "#000000";

	if (transparent) {
		progressSpan.style.opacity = 0;
		progressSpan.style.filter = "Alpha(opacity=1)";
		//progressSpan.style.opacity = 0.40;
		//progressSpan.style.filter = "Alpha(opacity=40)";
	} else {
		progressSpan.style.opacity = 0.40;
		progressSpan.style.filter = "Alpha(opacity=40)";
	} 
	
	//progressSpan.setAttribute("class","ModalShowDiv2");

	progressSpan.id = "opacity";



	document.body.appendChild(progressSpan);
	document.body.style.overflow = "hidden";
}
function removeOpacity() {
	if ($("opacity") == null) {
		return false;
	}
	document.body.removeChild($("opacity"));
	document.body.style.overflow = "auto";
}

/*
function setClass(el, className) {
	


	if (el.getAttributeNode("class")) {
  		for (var i = 0; i < el.attributes.length; i++) {
    		var attrName = el.attributes[i].name.toUpperCase();
    		if (attrName == 'CLASS') {
      			el.attributes[i].value = className;
    		}
  		}
	} else {
  		el.setAttribute("class", className);
	}

}
*/