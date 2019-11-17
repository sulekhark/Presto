CFLAGS:= -O3
# CFLAGS:= -pg
# CFLAGS:= -g

SOURCES:= main.cpp dfs_cycle_elim.cpp ../common/util.cpp ../common/framework.cpp
HEADERS:= main.h dfs_cycle_elim.h ../common/util.h ../common/framework.h

all : dfs_cycle_elim
	@echo
	@echo Build Successful!

dfs_cycle_elim : $(SOURCES) $(HEADERS) 
#	g++ -std=c++11 $(CFLAGS) -I ../common -I $$BASE_DIR/libs/boost_1_61_0 -march=native -Wall -Wextra -Werror $(SOURCES) -o dfs_cycle_elim
	g++ -std=c++11 $(CFLAGS) -I ../common -march=native -Wall -Wextra -Werror $(SOURCES) -o dfs_cycle_elim

clean : 
	rm -f dfs_cycle_elim
