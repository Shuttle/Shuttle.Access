const configuration = {
    url: process.env.VUE_APP_URL,
    environment: process.env.VUE_APP_ENVIRONMENT,

    debugging(){
        return this.environment.toLowerCase() === 'development';
    },

    getApiUrl(path){
        return this.url + path;
    }
}

if (Object.freeze){
    Object.freeze(configuration);
}

export default configuration;