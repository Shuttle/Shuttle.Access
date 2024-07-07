import moment from 'moment';
import configuration from '@/configuration';

export function useDateFormatter(value) {
    return moment(value).format((configuration.formats || {}).date || 'yyyy-MM-DD')
}