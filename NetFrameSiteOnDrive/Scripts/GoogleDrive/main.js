class Pass {
    constructor(id, user, pass, desc) {
        this.id = id;
        this.user = user;
        this.pass = pass;
        this.desc = desc;
        this.passInType = "password";
    }
}

function pass() {
    if (arguments.length === 1) {
        // assume input type is password json element
        var x = arguments[0]
        return new Pass(x.id, x.user, x.pass, x.desc);
    } else if (arguments.length === 4) {
        return new Pass(arguments[0], arguments[1], arguments[2], arguments[3]);
    }
    else {
        throw "Unexpected number of input parameters";
    }
}

var myApp = angular.module('myApp', []);
myApp.controller('mainCtrl', ['$scope', '$http', function ($scope, $http) {

    // Set the default value of inputType
    $scope.inputType = 'password';
    $scope.waitPassword = true
    $scope.tableContent = ''
    $scope.tableJson

    // Hide & show password function
    $scope.hideShowPassword = function () {
        if ($scope.inputType === 'password')
            $scope.inputType = 'text';
        else
            $scope.inputType = 'password';
    };

    $scope.okPassword = function (){
        $scope.waitPassword = false;
        $scope.viewCollection = [
            //pass("i1", "name1", "p1", "d1")
        ];
        // $scope.tableContent = "Now you see some content"
        $http.get("http://localhost:49906/GoogleDrive/table").then(
                function (successResponse) {
                    $scope.tableContent = successResponse.data;
                    $scope.tableJson = angular.fromJson($scope.tableContent);
                    angular.forEach($scope.tableJson.records, function(value, key) {
                        $scope.viewCollection.push(pass(value));
                    });
                },
                function (errorResponse) {
                    // handle errors here
                });
    }

}]);
