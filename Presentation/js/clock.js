var Clock = Class.create({
    initialize: function (id) {
        this.div = $(id);
        with (this.div) {
            this.showDate = readAttribute("data-show-date");
            this.showTime = readAttribute("data-show-time");
            this.faceType = readAttribute("data-face-type");
            this.panelWidth = readAttribute("data-panel-width");
            this.panelHeight = readAttribute("data-panel-height");
        }
        switch (this.faceType) {
            case '1':
                this.initAnalog();
                break;
            case '2':
                this.initDigital();
                break;
            default:
                break;
        }

        this.callBack();
        this.timer = setInterval(this.callBack.bind(this), 1000);
    }

    , initAnalog: function () {
        var s = this.panelWidth > this.panelHeight ? this.panelHeight : this.panelWidth;
        var face = new Element('ul', { "class": "analogFace", style: "background-size: " + s + "px " + s + "px;" })
        face.insert(this.elemSec = new Element('li', { "class": "analogSec", style: "width:" + s + "px; height:" + s + "px; background-size:" + s + "px " + s + "px;" }));
        face.insert(this.elemMin = new Element('li', { "class": "analogMin", style: "width:" + s + "px; height:" + s + "px; background-size:" + s + "px " + s + "px;" }));
        face.insert(this.elemHour = new Element('li', { "class": "analogHour", style: "width:" + s + "px; height:" + s + "px; background-size:" + s + "px " + s + "px;" }));
        this.div.style.width = this.div.style.height = s + "px";
        this.div.insert(face);
        //alert(this.div.innerHTML);
    }

    , initDigital: function () {
    }

    , callBack: function () {
        if (!this.div) {
            clearInterval(this.timer); return;
        }
        var time = moment();
        if (_canvas.offsetMilliseconds > 0)
            time.add('ms', _canvas.offsetMilliseconds);
        else
            time.subtract('ms', _canvas.offsetMilliseconds);

        switch (this.faceType) {
            case '0':
                var d = this.showDate ? time.format(_canvas.dateFormat) : "";
                var t = this.showTime ? time.format(_canvas.timeFormat) : "";
                this.div.innerHTML = d + (d != "" && t != "" ? "<br>" : "") + t;
                break;

            case '1':
                var sec = time.seconds();
                var min = time.minutes();
                var hrs = time.hours(); if (hrs > 12) hrs -= 12;
                this.rotateHand(this.elemSec, "rotate(" + (sec * 6) + "deg)");
                this.rotateHand(this.elemMin, "rotate(" + (min * 6) + "deg)");
                this.rotateHand(this.elemHour, "rotate(" + (hrs * 30 + (min / 2)) + "deg)");
                break;
        }
    }

    , rotateHand: function (e, r) {
        e.setStyle({ "transform": r, "-moz-transform": r, "-webkit-transform": r, "-ms-transform": r, "-o-transform": r });
    }
});

/*          setInterval( function() {
              var seconds = new Date().getSeconds();
              var sdegree = seconds * 6;
              var srotate = "rotate(" + sdegree + "deg)";
              
              $("#sec").css({"-moz-transform" : srotate, "-webkit-transform" : srotate});
                  
              }, 1000 );
              
         
              setInterval( function() {
              var hours = new Date().getHours();
              var mins = new Date().getMinutes();
              var hdegree = hours * 30 + (mins / 2);
              var hrotate = "rotate(" + hdegree + "deg)";
              
              $("#hour").css({"-moz-transform" : hrotate, "-webkit-transform" : hrotate});
                  
              }, 1000 );
        
        
              setInterval( function() {
              var mins = new Date().getMinutes();
              var mdegree = mins * 6;
              var mrotate = "rotate(" + mdegree + "deg)";
              
              $("#min").css({"-moz-transform" : mrotate, "-webkit-transform" : mrotate});
                  
              }, 1000 );*/