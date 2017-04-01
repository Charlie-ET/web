My Drive
DETAILS
ACTIVITY
Select a file or folder to view its details.

class Pass {
    constructor(id, user, pass, desc) {
        this.id = id;
        this.user = user;
        this.pass = pass;
        this.desc = desc;
    }
}

class PassRow {
    constructor(id, user, pass, desc) {
        this.passObj = new Pass(id, user, pass, desc);
        this.passInputType = "password";
    }
}

function passRow() {
    if (arguments.length === 1) {
        // assume input type is password json element
        var x = arguments[0]
        return new PassRow(x.id, x.user, x.pass, x.desc);
    } else if (arguments.length === 4) {
        return new PassRow(arguments[0], arguments[1], arguments[2], arguments[3]);
    } else if (arguments.length === 0) {
        return new PassRow("", "", "", "");
    }
    else {
        throw "Unexpected number of input parameters";
    }
}

var IV = [0];	
for (i = 0; i < 15; i++) {
    IV.push(0);
}

var twF = twofish(IV);

function padKeys(userKey) {
    while (userKey.length < 32) {
        userKey += "0";
    }
    return userKey;
}

function encrypt(userKey, plainText) {
    var userKey = padKeys(userKey);
    var keyArr = twF.stringToByteArray(userKey);
    var plainArr = twF.stringToByteArray(plainText);
    var encrypted = twF.encrypt(keyArr, plainArr);
    var b64encoded = btoa(String.fromCharCode.apply(null, encrypted));
    return b64encoded;
}

function decrypt(userKey, base64Encrypted) {
    var userKey = padKeys(userKey);
    var keyArr = twF.stringToByteArray(userKey);
    var encrypted = new Uint8Array(atob(base64Encrypted).split("").map(function (c) {
        return c.charCodeAt(0);
    }));

    var decrypted = twF.decrypt(keyArr, encrypted);
    var plainText = twF.byteArrayToString(decrypted);
    var index = plainText.lastIndexOf("}");
    var plainText = plainText.substring(0, index+1);

    return plainText;
}


var myApp = angular.module('myApp', []);
myApp.controller('mainCtrl', ['$scope', '$http', function ($scope, $http) {

    // Set the default value of inputType
    $scope.inputType = 'password';
    $scope.waitPassword = true;
    $scope.tableContent = '';
    $scope.tableJson;
    $scope.changed = false;
    $scope.log = '';
    $scope.isAdding = false;
    $scope.newpass = passRow();
    $scope.fileUndecrypted = '';
    $scope.showPasscodeError = false;

    //var test = {
    //    "id": "i1",
    //    "user": "u2",
    //    "pass": "p3",
    //    "desc": "d4"
    //};
    //$scope.log = 'posting test '; 
    //var config = {
    //    headers: {
    //        'Content-Type': 'application/json;charset=utf-8;'
    //    }
    //};
    //$http.post(
    //    '/GoogleDrive/EditPass',
    //    test,
    //    config).then(
    //    function (response) {
    //        // success callback
    //        $scope.log += "\nSucceeded";
    //    },
    //    function (response) {
    //        // failure callback
    //        $scope.log += "\nFailed";
    //    }
    //    );

    // Hide & show password function
    $scope.hideShowPassword = function () {
        if ($scope.inputType === 'password')
            $scope.inputType = 'text';
        else
            $scope.inputType = 'password';
    };

    $scope.okPassword = function () {
        $scope.viewCollection = [
            //passRow("i1", "name1", "p1", "d1")
        ];

        //var content = 'mytest';

        //var encrypted = encrypt($scope.passcode, content);
        //$scope.log += "Encrypted \n";
        //$scope.log += encrypted;

        //var config = {
        //    headers: {
        //        'Content-Type': 'application/json;charset=utf-8;'
        //    }
        //}
        ////var data = { "content": content };
        //var data = { "content": encrypted };
        //$http.post(
        //    'http://localhost:49906/GoogleDrive/SavePass',
        //    data,
        //    config).then(
        //    function (response) {
        //        // success callback
        //        $scope.log += "\nSucceeded";
        //    },
        //    function (response) {
        //        // failure callback
        //        $scope.log += "\nFailed";
        //    }
        //    );

        //return;

        $scope.waitPassword = false;

        // $scope.tableContent = "Now you see some content"
        $http.get("/GoogleDrive/table").then(
            function (successResponse) {
                $scope.fileUndecrypted = successResponse.data;

                if ($scope.fileUndecrypted === 'CREATE_NEW') {
                    $scope.tableJson = { "records": [] };
                    return;
                }


                // TODO: handle error
                try {
                    $scope.tableContent = decrypt($scope.passcode, successResponse.data);
                    $scope.tableJson = angular.fromJson($scope.tableContent);
                    angular.forEach($scope.tableJson.records, function (value, key) {
                        $scope.viewCollection.push(passRow(value));
                    });
                } catch (e) {
                    $scope.showPasscodeError = true;
                    $scope.passcodeError = e.message;
                    $scope.waitPassword = true;
                    return;
                }
            },
            function (errorResponse) {
                $scope.showPasscodeError = true;
                $scope.passcodeError = 'failed to open mima file';
                $scope.waitPassword = true;
                throw new 'Failed to open mima file';
            });
    };

    $scope.passChanged = function (idx) {
        $scope.changed = true;
        $scope.log = 'pass changed. idx is ' + idx + ' , new pass is ' + $scope.viewCollection[idx].passObj.pass;
        // MVC EditPass method input is null, because the jason 
        // element name is not double quoted when passing the 
        // 
        //  viewCollection[idx].passObj
        //
        // var config = {
        //    headers: {
        //        'Content-Type': 'application/json;charset=utf-8;'
        //    }
        //};
        //$http.post(
        //    'http://localhost:49906/GoogleDrive/EditPass',
        //    $scope.viewCollection[idx].passObj).then(
        //    function (response) {
        //        // success callback
        //        $scope.log += "\nSucceeded";
        //    },
        //    function (response) {
        //        // failure callback
        //        $scope.log += "\nFailed";
        //    }
        //    );
    };

    $scope.addClicked = function() {
        $scope.isAdding = true;
    };

    $scope.newPassItem = function () {
        $scope.viewCollection.push($scope.newpass);
        $scope.newpass = passRow();
        $scope.isAdding = false;
        $scope.changed = true;
    };

    $scope.saveToDrive = function () {
        $scope.log = 'submitting \n';
        $scope.tableJson.records = [];
        //$scope.log += $scope.viewCollection[0].passObj.user;
        angular.forEach($scope.viewCollection, function (value, key) {
            $scope.tableJson.records.push(value.passObj);
        });
        $scope.log += "tableJson new contents\n";

        var content = angular.toJson($scope.tableJson);
        $scope.log += content;

        // debug
        // content = 'mytest';

        var encrypted = encrypt($scope.passcode, content);
        $scope.log += "Encrypted \n";
        $scope.log += encrypted;

        var config = {
            headers: {
                'Content-Type': 'application/json;charset=utf-8;'
            }
        }
        //var data = { "content": content };
        var data = { "content": encrypted };
        $http.post(
            '/GoogleDrive/SavePass',
            data,
            config).then(
                function (response) {
                    // success callback
                    $scope.log += "\nSucceeded";
                },
                function (response) {
                    // failure callback
                    $scope.log += "\nFailed";
                }
            );
    };

}]);