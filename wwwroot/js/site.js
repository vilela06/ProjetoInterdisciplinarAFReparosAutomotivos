// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.
// Write your JavaScript code.

function showGenericModal(modalId) {
    var modalElement = document.getElementById(modalId);
    if (modalElement && typeof bootstrap !== 'undefined' && bootstrap.Modal) {
        var genericModal = new bootstrap.Modal(modalElement);
        genericModal.show();
    } else if (modalElement) {
        console.warn("Bootstrap JS não carregado ou ModalId inválido, não foi possível exibir o modal:", modalId);
    }
}