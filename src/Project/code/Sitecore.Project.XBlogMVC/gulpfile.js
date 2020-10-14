'use strict';

var gulp = require('gulp');
var sass = require('gulp-sass');
var concat = require('gulp-concat');
var uglify = require('gulp-uglify');

sass.compiler = require('node-sass');

gulp.task('sass:chartwell', function () {
    return gulp.src('./Components/XBlog Assets/scss/cwBlog.scss')
        .pipe(sass({ outputStyle: 'compressed' }).on('error', sass.logError))
        .pipe(gulp.dest('./Components/XBlog Assets/Style/'));
});

gulp.task('sass:chartwell:adaptive', function () {
    return gulp.src('./Components/XBlog Assets/scss/cwBlog-adaptive.scss')
        .pipe(sass({ outputStyle: 'compressed' }).on('error', sass.logError))
        .pipe(gulp.dest('./Components/XBlog Assets/Style/'));
});

gulp.task('sass:chartwell:watch', function () {
    gulp.watch('./Components/XBlog Assets/scss/**/*.scss', gulp.series('sass:chartwell', 'sass:chartwell:adaptive'));
});

