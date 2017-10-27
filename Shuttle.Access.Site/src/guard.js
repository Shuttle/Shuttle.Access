var guard = {
    againstUndefined: function(value, name) {
        if (value) {
            return;
        }

        throw new Error('\'' + name + '\' may not be undefined/null.');
    },

    againstMissingFunction: function(f, name) {
        this.againstUndefined(f, name);

        if (typeof f === 'function') {
            return;
        }

        throw new Error('\'' + name + '\' is not a function.');
    }
}

export default guard;