#!/usr/bin/env groovy
stage("Windows") {
  node('windows') {
    checkout poll: true, changelog: true, scm: scm
    bat ("git clean -xdff")
    bat ("Protobuild.exe --upgrade-all")
    bat ('Protobuild.exe --automated-build')
  }
}

/*
stage("Mac") {
  node('mac') {
    checkout poll: false, changelog: false, scm: scm
    sh ("git clean -xdff")
    sh ("mono Protobuild.exe --upgrade-all MacOS")
    sh ("mono Protobuild.exe --upgrade-all Android")
    sh ("mono Protobuild.exe --upgrade-all iOS")
    try {
      sh ("mono Protobuild.exe --automated-build")
      stash includes: '*.nupkg', name: 'mac-packages'
    } finally {
      publishHTML([allowMissing: true, alwaysLinkToLastBuild: true, keepAll: true, reportDir: 'Protogame.Tests/bin/MacOS/AnyCPU/Debug/VisualHTML', reportFiles: 'index.html', reportName: 'Visual Test Report - macOS'])
    }
  }
}

stage("Linux") {
  node('linux') {
    checkout poll: true, changelog: true, scm: scm
    sh ("git clean -xdff")
    sh ("mono Protobuild.exe --upgrade-all")
    try {
      sh ("mono Protobuild.exe --automated-build")
      stash includes: '*.nupkg', name: 'linux-packages'
    } finally {
      publishHTML([allowMissing: true, alwaysLinkToLastBuild: true, keepAll: true, reportDir: 'Protogame.Tests/bin/Linux/AnyCPU/Debug/VisualHTML', reportFiles: 'index.html', reportName: 'Visual Test Report - Linux'])
    }
  }
}
*/