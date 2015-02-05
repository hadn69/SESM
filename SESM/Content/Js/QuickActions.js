function initQA(serverID) {
    if ($("#QADiv") != null) {
        var serverName = "";
        function updateQA() {
            $.post("/API/Server/GetServer",
                {
                    ServerID: serverID
                },
                function(data, status) {
                    var root = $($.parseXML(data)).find("Response");

                    if (root.children("Type").text() == "Error") {
                        toastr.error(root.children("Content").text(), "Error " + root.children("ReturnCode").text());
                        return;
                    }

                    serverName = root.children("Content").children("Name").text();
                    var accessLevel = root.children("Content").children("AccessLevel").text();
                    $("#QADiv").hide();
                    if (!(accessLevel == "User" || accessLevel == "Guest" || accessLevel == "None")) {
                        $("#QADiv").show();

                        $("#btnQAStart").hide();
                        $("#btnQAStop").hide();
                        $("#btnQARestart").hide();
                        $("#btnQAKill").hide();

                        var state = root.children("Content").children("State").text();

                        if (state == "Unknow" || state == "Stopped") {
                            $("#btnQAStart").show();
                        }
                        if (state == "Unknow" || state == "Running") {
                            $("#btnQAStop").show();
                        }
                        if (state == "Unknow" || state == "Running") {
                            $("#btnQARestart").show();
                        }
                        if (state == "Unknow" || state == "Starting" || state == "Running" || state == "Stopping") {
                            $("#btnQAKill").show();
                        }
                    }
                });
        }

        setInterval(updateQA, 10000);
        updateQA();

        $("#btnQAStart").click(function() {
            $.post("/API/Server/StartServers/",
                {
                    ServerIDs: serverID
                },
                function(data, status) {
                    var root = $($.parseXML(data)).find("Response");

                    if (root.children("Type").text() == "Error") {
                        toastr.error(root.children("Content").text(), "Error " + root.children("ReturnCode").text());
                        return;
                    }
                    toastr.success(serverName + " has been asked to start", "Server Started");
                });
        });

        $("#btnQAStop").click(function () {
            $.post("/API/Server/StopServers/",
                {
                    ServerIDs: serverID
                },
                function (data, status) {
                    var root = $($.parseXML(data)).find("Response");

                    if (root.children("Type").text() == "Error") {
                        toastr.error(root.children("Content").text(), "Error " + root.children("ReturnCode").text());
                        return;
                    }
                    toastr.success(serverName + " has been asked to stop", "Server Stopped");
                });
        });

        $("#btnQARestart").click(function () {
            $.post("/API/Server/RestartServers/",
                {
                    ServerIDs: serverID
                },
                function (data, status) {
                    var root = $($.parseXML(data)).find("Response");

                    if (root.children("Type").text() == "Error") {
                        toastr.error(root.children("Content").text(), "Error " + root.children("ReturnCode").text());
                        return;
                    }
                    toastr.success(serverName + " has been asked to restart", "Server Restarted");
                });
        });

        $("#btnQAKill").click(function () {
            $.post("/API/Server/KillServers/",
                {
                    ServerIDs: serverID
                },
                function (data, status) {
                    var root = $($.parseXML(data)).find("Response");

                    if (root.children("Type").text() == "Error") {
                        toastr.error(root.children("Content").text(), "Error " + root.children("ReturnCode").text());
                        return;
                    }
                    toastr.success(serverName + " has been asked to restart", "Server Restarted");
                });
        });
    }
}