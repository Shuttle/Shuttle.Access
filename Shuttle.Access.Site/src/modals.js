import state from '~/state';

var modals = {
    confirm: function(message, callback) {
        state.modal.confirmation.message = message;
        state.modal.confirmation.primaryClick = function() {
            callback();
        };

        $('#modal-confirmation').modal({ show: true });
    }
}

export default modals;