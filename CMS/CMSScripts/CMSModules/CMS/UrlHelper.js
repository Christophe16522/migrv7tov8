﻿/**
 * UrlHelper module
 * Adds functionality for manipulation with URL and query.
 */

define(['Underscore'], function () {
    'use strict';
    
    // External underscore dependency
    var _ = window._,

        /**
         * Gets value of query parameter from given query string.
         *
         * @param {string} queryString - URL query string, leading question mark is optional
         * @param {string} name - Name of query parameter
         * @return {Object} Value of query parameter, if exist, null otherwise
         */
        getValue = function (queryString, name) {
            return getParams(queryString)[name];
        },


        /**
         * Gets string value of query parameter from given query string.
         *
         * @param {string} queryString - URL query string, leading question mark is optional
         * @param {string} name - Name of query parameter
         * @param {string} defaultValue - Default value if parameter does not exist
         * @return {string} Value of query parameter, if exist, default value otherwise
         */
        getString = function(queryString, name, defaultValue) {
            return getValue(queryString, name) || defaultValue;
        },
        

        /**
         * Sets value of query parameter for given query string. If parameter already exists, it is rewritten. If value is set to be null or undefined, parameter is removed.
         *
         * @param {string} queryString - URL query string, leading question mark is optional
         * @param {string} name - Name of query parameter
         * @param {string} value - Value to be set
         * @return {string} Updated query string
         */
        setValue = function(queryString, name, value) {
            var params = getParams(queryString);
            if (!value) {
                return removeParameter(queryString, name);
            }
            params[name] = value;
            return buildQueryString(params);
        },


        /**
         * Get object containing all query parameters from given query string.
         *
         * @param {string} queryString - URL query string, leading question mark is optional
         * @return {Object} Object containing key/value pairs representing query parameters
         */
        getParams = function (queryString) {
            // If whole url was passed, remove part before query string
            if (queryString.indexOf('?') !== -1) {
                queryString = getQueryString(queryString);
            }

            var result = {},
                regularExpression = /([^&=]+)=([^&]*)/g,
                temp;
            
            while (temp = regularExpression.exec(queryString)) {
                result[decodeURIComponent(temp[1])] = decodeURIComponent(temp[2]);
            }

            return result;
        },


        /**
         * Removes given parameters from given query string.
         *
         * @param {string} queryString - URL query string, leading question mark is optional
         * @param {Array} paramsToRemove - Array containing parameter names which should be removed
         * @return {string} update query string without removed parameters
         */
        removeParameters = function(queryString, paramsToRemove) {
            var params = getParams(queryString);
            return buildQueryString(_.omit(params, paramsToRemove));
        },
        

        /**
         * Removes given parameter from given query string.
         *
         * @param {string} queryString - URL query string, leading question mark is optional
         * @param {string} paramToRemove - Name of parameter to be removed
         * @return {string} update query string without removed parameter
         */
        removeParameter = function(queryString, paramToRemove) {
            return removeParameters(queryString, [paramToRemove]);
        },


        /**
         * Build query string from given parameters object.
         *
         * @param {Object} params - Object containing key/value pairs representing query parameters
         * @return {string} Empty string, if given params object does not have any property, built query string otherwise
         */
        buildQueryString = function (params) {
            var completedParams = [];
            
            for (var key in params) {
                if (params.hasOwnProperty(key)) {
                    completedParams.push(key + '=' + params[key]);
                }
            }
            
            if (completedParams.length) {
                return '?' + completedParams.join('&');
            }

            return '';
        },


        /**
         * Removes leading question mark from given query string, if present.
         * If whole URL is given, its query string is returned.
         *
         * @param {string} url - URL or URL query string, leading question mark is optional
         * @return {string} Query string without leading question mark
         */
        getQueryString = function (url) {
            var questionMarkPosition = url.indexOf('?');

            if (questionMarkPosition === -1) {
                return "";
            }

            return url.slice(questionMarkPosition + 1);
        };
    

    // Expose UrlHelper API
    return {
        getParameter: getValue,
        getParameterString: getString,
        setParameter: setValue,
        removeParameters: removeParameters,
        removeParameter: removeParameter,
        getParameters: getParams,
        buildQueryString: buildQueryString,
        getQueryString: getQueryString
    };
});
