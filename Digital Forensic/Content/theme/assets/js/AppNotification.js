var ThreallerMessage = (function (window, undefined) {

    function showNotification(title, msg, msgType,delay) {
        return new PNotify({
            title: title,
            text: msg,
            type: msgType,
            icon: 'ti ti-close',
            styling: 'fontawesome',
            delay: delay
        })
    }


    // explicitly return public methods when this object is instantiated
    return {
        showNotification: showNotification
        
    };

})(window);


