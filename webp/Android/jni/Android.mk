LOCAL_PATH := $(call my-dir)/../../

include $(CLEAR_VARS)

LOCAL_MODULE := snappy

LOCAL_CPP_EXTENSION := .cc

LOCAL_SRC_FILES := \
	snappy.cc \
	snappy-c.cc \
	snappy-sinksource.cc \
	snappy-stubs-internal.cc \

LOCAL_CFLAGS := -D__ANDROID__ -frtti -fexceptions
LOCAL_LDLIBS := -lc -lm -landroid -llog -frtti -fexceptions

include $(BUILD_SHARED_LIBRARY)
