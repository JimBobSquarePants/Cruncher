﻿/*global Autoprefixer */
var autoprefixerHelper = (function (autoprefixer, undefined) {
	'use strict';

	var exports = {},
		defaultOptions = {
			browsers: null,
			cascade: true,
			add: true,
			remove: true,
			supports: true,
			flexbox: true,
			grid: true,
			stats: null,
			map: false
		}
		;

	function mix(destination, source) {
		var propertyName;

		destination = destination || {};

		for (propertyName in source) {
			if (source.hasOwnProperty(propertyName)) {
				destination[propertyName] = source[propertyName];
			}
		}

		return destination;
	}

	exports.process = function (code, options) {
		var autoprefixOptions,
			browsers,
			result = {},
			processedCode = '',
			errors = [],
			message,
			lineNumber,
			columnNumber
			;

		options = options || {};

		autoprefixOptions = mix(mix({}, defaultOptions), options);

		browsers = autoprefixOptions.browsers;
		if (browsers && browsers.length > 0) {
			if (browsers[0].toLowerCase() === 'none') {
				browsers = [];
			}
		}
		else {
			browsers = null;
		}

		autoprefixOptions.browsers = browsers;

		try {
			processedCode = autoprefixer.process(code, autoprefixOptions).css;
		}
		catch (e) {
			if (typeof e.line !== 'undefined' || typeof e.autoprefixer !== 'undefined') {
				message = e.message;
				lineNumber = 0;
				columnNumber = 0;

				if (typeof e.line !== 'undefined') {
					lineNumber = e.line;
				}
				if (typeof e.column !== 'undefined') {
					columnNumber = e.column;
				}

				errors.push({
					'message': message,
					'lineNumber': lineNumber,
					'columnNumber': columnNumber
				});
			}
			else {
				throw (e);
			}
		}

		result.processedCode = processedCode;
		if (errors.length > 0) {
			result.errors = errors;
		}

		return JSON.stringify(result);
	};

	return exports;
} (Autoprefixer));