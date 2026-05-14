function showGenericModal(modalId) {
    var modalElement = document.getElementById(modalId);
    if (modalElement && typeof bootstrap !== 'undefined' && bootstrap.Modal) {
        var genericModal = new bootstrap.Modal(modalElement);
        genericModal.show();
    } else if (modalElement) {
        console.warn("Bootstrap JS não carregado ou ModalId inválido, não foi possível exibir o modal:", modalId);
    }
}
