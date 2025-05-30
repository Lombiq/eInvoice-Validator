/* eslint-disable import/no-unresolved -- ESLint does not know where to find external modules. */
const saxonJs = require('saxon-js');
const fs = require('fs');

const stylesheetCache = new Map();

function getOrLoadStylesheet(sefPath) {
    if (!stylesheetCache.has(sefPath)) {
        const sefJson = fs.readFileSync(sefPath, 'utf-8');
        const stylesheet = JSON.parse(sefJson);
        stylesheetCache.set(sefPath, stylesheet);
    }

    return stylesheetCache.get(sefPath);
}

module.exports = {
    validateAsText: async (convertedSchematronFilePath, xmlTextToValidate) => {
        try {
            const start = Date.now();
            const stylesheet = getOrLoadStylesheet(convertedSchematronFilePath);

            const result = await saxonJs.transform({
                stylesheetInternal: stylesheet,
                sourceText: xmlTextToValidate,
                sourceType: 'xml',
                destination: 'serialized',
            }, 'async');

            const duration = Date.now() - start;

            return {
                outputXml: result.principalResult,
                durationMs: duration,
            };
        }
        catch (error) {
            return 'An unexpected fatal error happened: ' + error;
        }
    },
};
