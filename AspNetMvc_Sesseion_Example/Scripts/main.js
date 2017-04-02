var myApp = angular.module('myApp', []);
myApp.controller('mainCtrl', ['$scope', '$http', function ($scope, $http) {
    $scope.peanut = 'You get a peanut.';
    $scope.logCount = 1;
    $scope.doLogging = function () {
        //var url = '/Peanut/LogDefault';
        var url = '/Peanut/Log?count=' + $scope.logCount;

        // HTTP request.
        $http.get(url)
            .then(function (response) {
                //First function handles success
                $scope.submitResult = response.data;
            }, function (response) {
                //Second function handles error
                $scope.submitResult = "Something went wrong";
            });
    }
}]);