import Component from 'can-component/';
import DefineMap from 'can-define/map/';
import view from './dashboard.stache!';
import resources from '~/resources';
import localisation from '~/localisation';
import Permissions from '~/permissions';
import state from '~/state';

localisation.addNamespace('dashboard');

resources.add('dashboard', { permission: Permissions.View.Dashboard });

export const ViewModel = DefineMap.extend({
    init () {
        state.title = localisation.value('dashboard:title');
    },
    message: {
        value: 'This is the sentinel-dashboard component'
    }
});

export default Component.extend({
    tag: 'sentinel-dashboard',
    view: view,
    viewModel: ViewModel
});

