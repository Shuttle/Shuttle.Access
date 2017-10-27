import $ from 'jquery';
import DefineMap from 'can-define/map/';
import DefineList from 'can-define/list/';
import loader from '@loader';
import alerts from '~/alerts';
import guard from '~/guard';
import each from 'can-util/js/each/';

$.ajaxPrefilter(function(options, originalOptions) {
    options.error = function(xhr) {
        if (xhr.responseJSON) {
            alerts.show({ message: xhr.responseJSON.message, type: 'danger', name: 'ajax-prefilter-error' });
        } else {
            alerts.show({ message: xhr.status + ' / ' + xhr.statusText, type: 'danger', name: 'ajax-prefilter-error' });
        }

        if (originalOptions.error) {
            originalOptions.error(xhr);
        }
    };
});

let parameterExpression = /\{.*?\}/g;

let Api = DefineMap.extend(
    'Api',
    {
        options: { value: {} },
        working: { type: 'boolean', value: false },

        init (options) {
            guard.againstUndefined(options, 'options');

            this.options = (typeof options === 'string' || options instanceof String)
                ? { endpoint: options }
                : options;

            guard.againstUndefined(this.options.endpoint, 'options.endpoint');

            if (!this.options.cache) {
                this.options.cache = false;
            }
        },

        _call (options) {
            return new Promise((resolve, reject) => {
                try {
                    const o = options || {};
                    const parsedEndpoint = this.parseEndpoint(this.options.endpoint, o.parameters);
                    const ajax = {
                        url: parsedEndpoint.url,
                        type: o.method,
                        async: true,
                        beforeSend: o.beforeSend,
                        timeout: o.timeout || 60000
                    };

                    switch (o.method.toLowerCase()) {
                        case 'get':
                        {
                            ajax.cache = this.options.cache;
                            ajax.dataType = 'json';
                            break;
                        }
                        case 'post':
                        case 'put':
                        {
                            ajax.data = JSON.stringify(o.data || {});
                            ajax.contentType = 'application/json';
                            break;
                        }
                    }

                    $.ajax(ajax)
                        .done(function(response) {
                            resolve(response);
                        })
                        .fail(function(jqXHR, textStatus, errorThrown) {
                            reject(new Error(errorThrown));
                        });
                } catch (e) {
                    reject(e);
                }
            });
        },

        parseEndpoint (endpoint, parameters) {
            guard.againstUndefined(endpoint, 'endpoint');

            const p = parameters || {};
            const params = [];
            let match;

            do {
                match = parameterExpression.exec(endpoint);

                if (match) {
                    const name = match[0];

                    if (name.length < 3) {
                        throw new Error($
                            `Endpoint '{endpoint}' contains parameter '{name}' that is not at least 3 characters in length.`);
                    }

                    params.push({
                        name: name.substr(1, name.length - 2),
                        index: match.index
                    });
                }
            } while (match);

            let url = endpoint.indexOf('http') < 0 ? loader.serviceBaseURL + endpoint : endpoint;

            each(params,
                function(param) {
                    url = url.replace(`{${param.name}}`, !!p[param.name] ? p[param.name] : '');
                });

            return {
                url: url,
                parameters: params
            };
        },

        post (data) {
            guard.againstUndefined(data, 'data');

            const self = this;
            this.working = true;

            return this._call({
                    data: data,
                    method: 'POST'
                })
                .then(function(response) {
                    self.working = false;

                    return response;
                })
                .catch(function(error) {
                    self.working = false;

                    return error;
                });
        },

        put (data) {
            guard.againstUndefined(data, 'data');

            const self = this;
            this.working = true;

            return this._call({
                    data: data,
                    method: 'POST'
                })
                .then(function(response) {
                    self.working = false;

                    return response;
                })
                .catch(function(error) {
                    self.working = false;

                    return error;
                });
        },

        item (parameters) {
            const self = this;
            this.working = true;

            return this._call({
                    method: 'GET',
                    parameters: parameters
                })
                .then(function(response) {
                    self.working = false;

                    return !!self.options.Map
                        ? new self.options.Map(response)
                        : new DefineMap(response);
                })
                .catch(function(error) {
                    self.working = false;

                    return error;
                });
        },

        list (parameters) {
            const self = this;
            this.working = true;

            return this._call({
                    method: 'GET',
                    parameters: parameters
                })
                .then(function(response) {
                    self.working = false;

                    if (!response.data) {
                        return response;
                    }

                    const result = !!self.options.List
                        ? new self.options.List()
                        : new DefineList();

                    each(response.data,
                        (item) => {
                            result.push(!!self.options.Map
                                ? new self.options.Map(item)
                                : new DefineMap(item));
                        });

                    return result;
                })
                .catch(function(error) {
                    self.working = false;

                    return error;
                });
        },

        'delete' (parameters) {
            guard.againstUndefined(parameters, 'parameters');

            const self = this;
            this.working = true;

            return this._call({
                    method: 'DELETE',
                    parameters: parameters
                })
                .then(function(response) {
                    self.working = false;

                    return response;
                })
                .catch(function(error) {
                    self.working = false;

                    return error;
                });
        }

    }
);

export default Api;