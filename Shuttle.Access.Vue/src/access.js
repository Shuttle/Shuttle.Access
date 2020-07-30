import configuration from './configuration';
import Access from 'shuttle-access';

const access = new Access(configuration.url);

export default access;