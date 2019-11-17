# CFLAGS:= -O3
# CFLAGS:= -pg
CFLAGS:= -g

SOURCES:= main.cpp get_coreachable.cpp ../common/util.cpp ../common/framework.cpp ../common/analysis.cpp
HEADERS:= main.h get_coreachable.h ../common/util.h ../common/framework.h ../common/analysis.h

all : get_coreachable
	@echo
	@echo Build Successful!

get_coreachable : $(SOURCES) $(HEADERS) 
#	g++ -std=c++11 $(CFLAGS) -I ../common -I $$BASE_DIR/libs/boost_1_61_0 -march=native -Wall -Wextra -Werror $(SOURCES) -o get_coreachable
	g++ -std=c++11 $(CFLAGS) -I ../common -march=native -Wall -Wextra -Werror $(SOURCES) -o get_coreachable

clean : 
	rm -f get_coreachable
