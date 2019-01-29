/**
 * Module for perform some javascript tasks" 
 *
 * @author Mark Adesina <omark@simplexsystem.com>
 */

var SiteUtils = SiteUtils || (function ($) {
    'use strict';

    // Creating modal dialog's DOM
    var $dialog = $('<div id="markmodalloadwrap" style="display:block" class="loadmodal">' +
        '<div class="modalInner">' +
        '<div id="loadingMessage" class="cssload-loader"><img src="/Content/images/loading.gif" alt=""></div>' +
        '</div></div>');

    var utilsFunc = {

        //Format Number
        formatNumber: function (num) {
            if (num == undefined) {
                return 0;
            }
            return num.toString().replace(/(\d)(?=(\d{3})+(?!\d))/g, "$1,");
        },
        removeFormat: function (num) { //Remove Number Formatting
            return parseFloat(num.replace(/[^0-9-.]/g, ''));
        },

        showLoading: function (message) { //Show Loading Icon
            if (typeof message === 'undefined') {
                message = 'Loading....';
            }

            $dialog.appendTo('body');
        },
        hideLoading: function () {
            $('body').find($("#markmodalloadwrap")).remove();
        }
    }

    return utilsFunc;



})(jQuery);