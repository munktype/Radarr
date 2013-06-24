﻿﻿'use strict';
define(
    [
        'Series/EpisodeModel',
        'backbone.pageable'
    ], function (EpisodeModel, PagableCollection) {
        return PagableCollection.extend({
            url  : window.ApiRoot + '/missing',
            model: EpisodeModel,

            state: {
                pageSize: 15,
                sortKey : 'airDate',
                order   : 1
            },

            queryParams: {
                totalPages  : null,
                totalRecords: null,
                pageSize    : 'pageSize',
                sortKey     : 'sortKey',
                order       : 'sortDir',
                directions  : {
                    '-1': 'asc',
                    '1' : 'desc'
                }
            },

            parseState: function (resp) {
                return {totalRecords: resp.totalRecords};
            },

            parseRecords: function (resp) {
                if (resp) {
                    return resp.records;
                }

                return resp;
            }
        });
    });
