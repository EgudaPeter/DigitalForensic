/**
 * Module for displaying "Css Loading Modal" 
 *
 * @author Mark Adesina <mark2kk@gmail.com>
 */

var loadingDialog = loadingDialog || (function ($) {
    'use strict';

    // Creating modal dialog's DOM
    var $dialog = $('<div id="markmodalloadwrap" style="display:block" class="loadmodal">' +
        '<div class="modalInner">' +
        '<div id="loadingMessage" class="cssload-loader"><img src="/Content/assets/img/loading.gif" alt=""></div>' +
        '</div></div>');

    var loader = {

        show: function (message) {
            if (typeof message === 'undefined') {
                message = 'Loading....';
            }
            
            $dialog.appendTo('body');
           

        },
        hide: function () {
            
            $('body').find($("#markmodalloadwrap")).remove();
        }
    }

    return loader;

    

})(jQuery);