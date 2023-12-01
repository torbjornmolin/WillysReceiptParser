docker run -d \
	--name postgres-willyskvitton\
	-e POSTGRES_PASSWORD=willys\
	-e PGDATA=/var/lib/postgresql/data/pgdata \
	-v ./postgres-data:/var/lib/postgresql/data \
	postgres
