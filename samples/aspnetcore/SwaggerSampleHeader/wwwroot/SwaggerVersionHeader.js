$(function () {
    window.version = $("#select_document option:selected").text().substring(1);
    window.swaggerUi.api.clientAuthorizations.add("api-version", new SwaggerClient.ApiKeyAuthorization('api-version', window.version, 'header'));
});