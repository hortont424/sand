#!/usr/bin/env python

from os import system

def options(opt):
    opt.tool_options('compiler_cxx compiler_c')

def configure(conf):
    conf.check_tool('compiler_cxx compiler_c')

    library_check(conf)

def library_check(conf):
    libraries = (
        ("libglog", "GLOG", None),
    )

    for package, uselib, version in libraries:
        conf.check_cfg(package=package, uselib_store=uselib,
                       mandatory=True, version=version,
                       args='--cflags --libs')

def build(bld):
    bld.add_subdirs("src")
    bld.add_group()

def run(ctx):
    if system("./waf build") is 0:
        return system("GLOG_logtostderr=1 ./build/src/game")

def db(ctx):
    if system("./waf build") is 0:
        return system("GLOG_logtostderr=1 gdb ./build/src/game")
