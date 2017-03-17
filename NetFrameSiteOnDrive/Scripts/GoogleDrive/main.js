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
        // $scope.tableContent = "Now you see some content"
        $http.get("http://localhost:49906/GoogleDrive/table").then(
                function (successResponse) {
                    $scope.tableContent = successResponse.data;
                    $scope.tableJson = angular.fromJson($scope.tableContent)
                    $scope.firstuser = $scope.tableJson.records[0].user
                },
                function (errorResponse) {
                    // handle errors here
                });
    }
}]);
