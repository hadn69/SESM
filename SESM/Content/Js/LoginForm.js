var Login = function () {
    var handleLogin = function () {
        $('.login-form').validate({
            errorElement: 'span', //default input error message container
            errorClass: 'help-block', // default input error message class
            focusInvalid: false, // do not focus the last invalid input
            rules: {
                username: {
                    required: true
                },
                password: {
                    required: true
                }
            },

            messages: {
                username: {
                    required: "Username is required."
                },
                password: {
                    required: "Password is required."
                }
            },

            invalidHandler: function (event, validator) { //display error alert on form submit   
                $('#LoginError').show();
                $("#LoginErrorText").html("<i class=\"fa fa-angle-down\"></i> Error, see below <i class=\"fa fa-angle-down\"></i>");
            },

            highlight: function (element) { // hightlight error inputs
                $(element)
                    .closest('.form-group').addClass('has-error'); // set error class to the control group
            },

            success: function (label) {
                label.closest('.form-group').removeClass('has-error');
                label.remove();
            },

            errorPlacement: function (error, element) {
                error.insertAfter(element.closest('.input-icon'));
            },

            submitHandler: function (form) {
                requestAuth();
                return false;
            }
        });

        $('.login-form input').keypress(function (e) {
            if (e.which == 13) {
                if ($('.login-form').validate().form()) {
                    requestAuth();
                }
            }
        });

        function requestAuth() {
            $.post("/API/Account/GetChallenge",
                {},
                function (data, status) {
                    if (status != "success") {
                        $("#LoginErrorText").html("Error Occured (see console for more infos).");
                        $('#LoginError').show();
                        Console.warn("Fatal Error : ");
                        Console.warn("Status : " + status);
                        Console.warn("Data : " + data);
                        return;
                    }

                    var xmlDoc = $.parseXML(data);
                    var xmlObj = $(xmlDoc);
                    var root = xmlObj.find("Response");

                    if (root.children("Type").text() == "Error") {
                        $("#LoginErrorText").html(root.children("Content").text());
                        $('#LoginError').show();
                        return;
                    }

                    var challenge = root.children("Content").text();
                    var password = CryptoJS.SHA1(challenge + CryptoJS.MD5($("#LoginPassword").val()).toString());

                    $.post("/API/Account/Authenticate",
                        {
                            Login: $("#LoginUsername").val(),
                            password: password.toString()
                        },
                        function (data, status) {
                            if (status != "success") {
                                $("#LoginErrorText").html("Error Occured (see console for more infos).");
                                $('#LoginError').show();
                                Console.warn("Fatal Error : ");
                                Console.warn("Status : " + status);
                                Console.warn("Data : " + data);
                                return;
                            }
                            var xmlDoc = $.parseXML(data);
                            var xmlObj = $(xmlDoc);
                            var root = xmlObj.find("Response");
                            if (root.children("Type").text() == "Error") {
                                $("#LoginErrorText").html(root.children("Content").text());
                                $('#LoginError').show();
                                return;
                            }
                            window.location.href = "/Server";
                        });
                });
        }
    }

    var handleRegister = function () {
        $('.register-form').validate({
            errorElement: 'span', //default input error message container
            errorClass: 'help-block', // default input error message class
            focusInvalid: false, // do not focus the last invalid input
            ignore: "",
            rules: {
                username: {
                    required: true
                },
                email: {
                    required: true,
                    email: true
                },
                password: {
                    required: true
                },
                rpassword: {
                    equalTo: "#RegisterPassword"
                }
            },
            messages: {
                username: {
                    required: "Username is required."
                },
                email: {
                    required: "EMail is required."
                },
                password: {
                    required: "Password is required."
                },
                rpassword: {
                    required: "Password is required."
                }
            },

            invalidHandler: function (event, validator) { //display error alert on form submit   
                $('#RegisterError').show();
                $("#RegisterErrorText").html("<i class=\"fa fa-angle-down\"></i> Error, see below <i class=\"fa fa-angle-down\"></i>");
            },

            highlight: function (element) { // hightlight error inputs
                $(element)
                    .closest('.form-group').addClass('has-error'); // set error class to the control group
            },

            success: function (label) {
                label.closest('.form-group').removeClass('has-error');
                label.remove();
            },

            errorPlacement: function (error, element) {
                if (element.attr("name") == "tnc") { // insert checkbox errors after the container                  
                    error.insertAfter($('#register_tnc_error'));
                } else if (element.closest('.input-icon').size() === 1) {
                    error.insertAfter(element.closest('.input-icon'));
                } else {
                    error.insertAfter(element);
                }
            },

            submitHandler: function (form) {
                requestRegister();
            }
        });

        $('.register-form input').keypress(function (e) {
            if (e.which == 13) {
                if ($('.register-form').validate().form()) {
                    requestRegister();
                }
            }
        });

        function requestRegister() {
            $.post("/API/Account/Register",
            {
                Login: $("#RegisterUsername").val(),
                Password: CryptoJS.MD5($("#RegisterPassword").val()).toString(),
                Email: $("#RegisterEMail").val()
            },
                function (data, status) {
                    if (status != "success") {
                        $("#RegisterErrorText").html("Error Occured (see console for more infos).");
                        $('#RegisterError').show();
                        Console.warn("Fatal Error : ");
                        Console.warn("Status : " + status);
                        Console.warn("Data : " + data);
                        return;
                    }

                    var xmlDoc = $.parseXML(data);
                    var xmlObj = $(xmlDoc);
                    var root = xmlObj.find("Response");

                    if (root.children("Type").text() == "Error") {
                        $("#RegisterErrorText").html(root.children("Content").text());
                        $('#RegisterError').show();
                        return;
                    }
                    window.location.href = "/Server";
                });
        }



        jQuery('#register-btn').click(function () {
            jQuery('.login-form').hide();
            jQuery('.register-form').show();
        });

        jQuery('#register-back-btn').click(function () {
            jQuery('.login-form').show();
            jQuery('.register-form').hide();
        });
    }

    return {
        //main function to initiate the module
        init: function () {
            handleLogin();
            handleRegister();
        }
    };
}();