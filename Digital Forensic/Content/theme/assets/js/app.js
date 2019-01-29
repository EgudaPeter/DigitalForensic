var UserAccounts = {

    init: function (config) {
        this.config = config;
        this.bindEvents();
        this.setupTempalates();
        this.fetchAllUsers();
    },
    setupTempalates: function () {
        this.config.usersListTemplate = Handlebars.compile(this.config.userTemplate);
    },
    bindEvents: function () {
        this.config.saveButton.on('click', this.saveUser);
    },

    fetchAllUsers: function () {
        var self = UserAccounts;
        $.ajax({
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            type: 'GET',
            url: this.config.getUsersUrl,
            success: function (res) {
                self.config.userTableBody.append(self.config.usersListTemplate(res.results));
            }
        });
    },
    saveUser: function () {
        var self = UserAccounts;
    }


}