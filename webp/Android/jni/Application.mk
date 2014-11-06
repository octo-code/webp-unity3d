NDK_PROJECT_PATH := $(call my-dir)/..

APP_ABI := armeabi-v7a
APP_STL := gnustl_static
APP_CFLAGS := -O3 -DNDEBUG -g 
APP_PLATFORM := android-15
APP_PIE := false