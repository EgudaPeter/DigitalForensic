/**
 * Module for perform some javascript tasks" 
 *'<img src="/multiplan/Content/loader/img/ibs-logo.png" alt=""><p>' + msg + '</p></div>' +
  '<img src="/Content/loader/img/ibs-logo.png" alt=""><p>' + msg + '</p></div>' +
 * @author Mark Adesina <omark@simplexsystem.com>
 */

var SiteUtils = SiteUtils || (function ($) {
    'use strict';

    // Creating modal dialog's DOM
    var $dialog;

    function dialogContent(msg) {
        $dialog = $('<div id="ibsLoading" style="display:block" class="loadmodal">' +
                 '<div class="modalInner">' +
                 '<div id="loadingMessage" class="cssload-loader">' +
                 '<div></div><p>' + msg + '</p></div>' +
                 '</div></div>');

        return $dialog;
    }




    var utilsFunc = {

        loading: function (message) { //Show Loading Icon
            if (typeof message === 'undefined') {
                message = 'Loading....';
            }

            dialogContent(message).appendTo('body');
        },
        loadingOff: function () {
            $('body').find($("#ibsLoading")).remove();
        },

        createGuid: function () {
            return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                var r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
                return v.toString(16);
            });
        }
    }


    return utilsFunc;



})(jQuery);