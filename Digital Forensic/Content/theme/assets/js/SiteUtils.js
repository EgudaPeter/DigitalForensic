/**
 * Module for displaying "Css Loading Modal" 
 *
 * @author Mark Adesina <mark2kk@gmail.com>
 */

var SiteUtils = SiteUtils || (function ($) {
    'use strict';

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
        }
    }

    return utilsFunc;



})(jQuery);