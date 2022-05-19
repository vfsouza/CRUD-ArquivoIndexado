using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Trabalho2.Environment;
using Trabalho2.Models;

namespace Trabalho2 {
	public class FileHandler {

		// CRUD (Create, Read, Update, Delete) do Banco de Dados de ContaBancaria.

		/// <summary>
		/// Caminho para o arquivo Conta.db.
		/// </summary>
		private string pathContaDB = EnvironmentVar.DBPath;

		/// <summary>
		/// Caminho para o arquivo Index.db.
		/// </summary>
		private string pathIndexDB = EnvironmentVar.IndexPath;

		/// <summary>
		/// Caminho para o arquivo InvertedCity.db.
		/// </summary>
		private string pathInvertedCityDB = EnvironmentVar.InvertedCityPath;

		/// <summary>
		/// Caminho para o arquivo InvertedName.db.
		/// </summary>
		private string pathInvertedNameDB = EnvironmentVar.InvertedNamePath;

		/// <summary>
		/// Cria um novo registro no Banco de Dados.
		/// </summary>
		/// <param name="conta">Conta a ser inserida no Banco de Dados.</param>
		/// <returns>Um <see cref="Boolean"/>. Se <see langword="true"/>, então o registro foi atualizado com sucesso. Se <see langword="false"/>, o registro não foi atualizado.</returns>
		public bool Create(ContaBancaria conta) {

			long pos;

			if (conta == null) {
				return false;
			}

			// Lê o ID máximo no início do arquivo.
			using (FileStream fs = new FileStream(pathContaDB, FileMode.Open, FileAccess.Read)) {
				using (BinaryReader br = new BinaryReader(fs)) {
					br.BaseStream.Seek(0, SeekOrigin.Begin);
					conta.IdConta = (ushort)(br.ReadUInt16() + 1);
				}

				// Faz o update do ID máximo no início do arquivo.
				UpdateMaxId(conta.IdConta);
			}

			using (FileStream fs = new FileStream(pathContaDB, FileMode.Open, FileAccess.Write)) {
				using (BinaryWriter bw = new BinaryWriter(fs)) {
					bw.Seek(0, SeekOrigin.End);
					pos = bw.BaseStream.Position;
					bw.Write(false);
					bw.Write(conta.Serialize().Length);
					bw.Write(conta.Serialize());
				}
			}
			CreateIndex(conta.IdConta, pos);
			CreateInvertedFiles();
			return true;
		}

		/// <summary>
		/// Marca a <see cref="ContaBancaria.Lapide"/> como <see langword="true"/>. Ou seja, a conta vai ser indicada como: excluída.
		/// </summary>
		/// <param name="contaId">O ID da conta a ser excluída</param>
		/// <returns><see cref="ContaBancaria"/> que foi deletada.</returns>
		public bool DeleteById(ushort contaId) {
			long pos = 0;
			using (FileStream fs = new FileStream(pathContaDB, FileMode.Open, FileAccess.ReadWrite)) {
				using (BinaryReader br = new BinaryReader(fs)) {
					br.BaseStream.Seek(2, SeekOrigin.Begin);

					// Roda um loop até encontrar o ID da conta procurada
					while (br.PeekChar() != -1) {
						pos = br.BaseStream.Position;
						bool lapide = br.ReadBoolean();
						int tam = br.ReadInt32();

						if (!lapide) {
							ushort id = br.ReadUInt16();
							if (id == contaId) {
								using (BinaryWriter bw = new BinaryWriter(fs)) {
									bw.BaseStream.Position = pos;
									bw.Write(true);
									return true;
								}
							} else {
								br.BaseStream.Position += tam - 2;
							}
						} else {
							br.BaseStream.Position += tam;
						}
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Retorna uma <see cref="ContaBancaria"/> requisitada com base no <paramref name="contaId"></paramref>
		/// </summary>
		/// <param name="contaId">ID a ser procurado no Banco de Dados</param>
		/// <returns>Uma <see cref="ContaBancaria"/> com os dados encontrados no Banco de Dados</returns>
		public ContaBancaria? ReadById(ushort contaId) {
			ContaBancaria conta = new ContaBancaria();
			using (FileStream fs = new FileStream(pathContaDB, FileMode.Open, FileAccess.Read)) {
				long length = fs.Length;
				using (BinaryReader br = new BinaryReader(fs)) {
					br.BaseStream.Seek(2, SeekOrigin.Begin);
					while (br.PeekChar() != -1) {
						conta.Lapide = br.ReadBoolean();
						int tam = br.ReadInt32();

						conta.Deserialize(br);

						if (conta.IdConta == contaId) {
							return conta;
						} else {
							br.BaseStream.Position += tam;
						}
					}
				}
			}
			return null;
		}

		public List<ContaBancaria> ReadAll() {
			List<ContaBancaria> list = new List<ContaBancaria>();

			using (FileStream fs = new FileStream(pathIndexDB, FileMode.Open, FileAccess.Read)) {
				using (BinaryReader br = new BinaryReader(fs)) {
					while (br.PeekChar() != -1) {
						ContaBancaria? conta = new ContaBancaria();

						br.ReadUInt16();
						conta = ReadByPos(br.ReadInt64());	
						if (conta != null && !conta.Lapide) {
							list.Add(conta);
						}
					}
				}
			}

			return list;
		}

		public List<ushort> ReadAll(string str) {
			List<ushort> list = new List<ushort>();

			using (FileStream fs = new FileStream(pathIndexDB, FileMode.Open, FileAccess.Read)) {
				using (BinaryReader br = new BinaryReader(fs)) {
					while (br.PeekChar() != -1) {
						ContaBancaria? conta = new ContaBancaria();

						br.ReadUInt16();
						conta = ReadByPos(br.ReadInt64());
						if (conta != null && !conta.Lapide) {
							if (str.Length == 2 && str == conta.Cidade) {
								list.Add(conta.IdConta);
							}
							if (str == conta.NomePessoa) {
								list.Add(conta.IdConta);
							}
						}
					}
				}
			}
			return list;
		}

		/// <summary>
		/// Lê o registro no arquivo Conta.db com base em uma posição passada.
		/// </summary>
		/// <param name="pos">Posição no arquivo Conta.db.</param>
		/// <returns><see cref="ContaBancaria"/> desserializada com base no registro encontrado</returns>
		public ContaBancaria? ReadByPos(long pos) {
			ContaBancaria conta = new ContaBancaria();
			using (FileStream fs = new FileStream(pathContaDB, FileMode.Open, FileAccess.Read)) {
				using (BinaryReader br = new BinaryReader(fs)) {
					br.BaseStream.Position = pos;
					conta.Lapide = br.ReadBoolean();
					br.ReadInt32();
					conta.Deserialize(br);
					return conta;
				}
			}
		}

		public List<ContaBancaria> ReadByCity(string city) {
			List<ushort> ids = new List<ushort>();
			List<ContaBancaria> contas = new List<ContaBancaria>();

			using (FileStream fs = new FileStream(pathInvertedCityDB, FileMode.Open, FileAccess.Read)) {
				using (BinaryReader br = new BinaryReader(fs)) {
					br.BaseStream.Seek(0, SeekOrigin.Begin);
					while (br.PeekChar() != -1) {
						string cidade = br.ReadString();
						int length = br.ReadInt32();

						if (city == cidade) {
							for (int i = 0; i < length; i++) {
								ids.Add(br.ReadUInt16());
							}
						} else {
							br.BaseStream.Position += length * 2;
						}
					}
				}
			}

			for (int i = 0; i < ids.Count; i++) {
				contas.Add(ReadByPos(FindPosByIndex(ids.ElementAt(i))));
			}

			return contas;
		}

		public List<ContaBancaria> ReadByName(string name) {
			List<ushort> ids = new List<ushort>();
			List<ContaBancaria> contas = new List<ContaBancaria>();

			using (FileStream fs = new FileStream(pathInvertedNameDB, FileMode.Open, FileAccess.Read)) {
				using (BinaryReader br = new BinaryReader(fs)) {
					while (br.PeekChar() != -1) {
						string nome = br.ReadString();
						int length = br.ReadInt32();
						if (name.CompareTo(nome) == 0) {
							for (int i = 0; i < length; i++) {
								ids.Add(br.ReadUInt16());
							}
						} else {
							br.BaseStream.Position += length * 2;
						}
					}
				}
			}

			for (int i = 0; i < ids.Count; i++) {
				contas.Add(ReadByPos(FindPosByIndex(ids.ElementAt(i))));
			}

			return contas;
		}

		/// <summary>
		/// Faz o update de uma ContaBancaria no Banco de Dados.
		/// </summary>
		/// <param name="contaNova">Nova conta a ser substituída pela existente no Banco de Dados.</param>
		public void UpdateById(ContaBancaria contaNova, ushort contaId) {

			contaNova.IdConta = contaId;
			byte[] buffer = contaNova.Serialize();
			long pos;

			using (FileStream fs = new FileStream(pathContaDB, FileMode.Open, FileAccess.ReadWrite)) {
				using (BinaryReader br = new BinaryReader(fs)) {
					br.BaseStream.Seek(2, SeekOrigin.Begin);
					while (br.PeekChar() != -1) {
						bool lapide = br.ReadBoolean();
						int tam = br.ReadInt32();

						pos = br.BaseStream.Position;
						ushort id = br.ReadUInt16();

						if (id == contaId) {
							if (tam < buffer.Length) {
								DeleteById(contaId);
								Create(contaNova);
								break;
							} else {
								using (BinaryWriter bw = new BinaryWriter(fs)) {
									bw.BaseStream.Position = pos;
									bw.Write(buffer);
								}
								break;
							}
						} else {
							br.BaseStream.Position += tam - 2;
						}
					}
				}
			}
		}

		/// <summary>
		/// Faz o update do ID (último ID) no início do arquivo com base no ID do último registro inserido.
		/// </summary>
		/// <param name="id">ID do último registro</param>
		/// <returns>Último ID</returns>
		public void UpdateMaxId(ushort id) {
			using (FileStream fs = new FileStream(pathContaDB, FileMode.Open)) {
				using (BinaryWriter bw = new BinaryWriter(fs)) {
					bw.Seek(0, SeekOrigin.Begin);
					bw.Write(id);
				}
			}
		}

		/// <summary>
		/// Cria o arquivo inicial Conta.db para ser usado como Banco de Dados (Arquivo Sequencial) temporário
		/// </summary>
		public void BuildFile() {

			// Cria o arquivo de indíces
			BuildIndexFile();

			// Cria os arquivos invertidos
			BuildInvertedFiles();

			// Verifica se o arquivo existe ou não. Caso não exista, cria o arquivo e coloca o maxId como 0.
			// Caso o arquivo já exista, verifica se o arquivo só contém o maxId escrito nele e se ele é igual a zero e reescreve
			if (!File.Exists(pathContaDB)) {
				using (FileStream fs = new FileStream(pathContaDB, FileMode.Create)) {
					using (BinaryWriter bw = new BinaryWriter(fs)) {
						bw.Seek(0, SeekOrigin.Begin);
						bw.Write((ushort)0);
					}
				}
			} else {
				using (FileStream fs = new FileStream(pathContaDB, FileMode.Open, FileAccess.ReadWrite)) {
					using (BinaryReader br = new BinaryReader(fs)) {
						br.BaseStream.Seek(0, SeekOrigin.Begin);
						long length = fs.Length;
						if (length == 0) {
							using (BinaryWriter bw = new BinaryWriter(fs)) {
								bw.Write((ushort)0);
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Cria o arquivo Index.db caso ele não exista.
		/// </summary>
		private void BuildIndexFile() {
			if (!File.Exists(pathIndexDB)) {
				File.Create(pathIndexDB);
			}
		}

		/// <summary>
		/// Cria o arquivo InvertedCity.db e InvertedName.db caso não existam.
		/// </summary>
		private void BuildInvertedFiles() {
			if (!File.Exists(pathInvertedNameDB)) {
				File.Create(pathInvertedNameDB);
			}
			if (!File.Exists(pathInvertedCityDB)) {
				File.Create(pathInvertedCityDB);
			}
		}

		/// <summary>
		/// Cria um indíce no arquivo Index.db.
		/// </summary>
		/// <param name="id">ID do registro</param>
		/// <param name="pos">Posição do registro</param>
		public void CreateIndex(ushort id, long pos) {
			using (FileStream fs = new FileStream(pathIndexDB, FileMode.Open, FileAccess.Write)) {
				using (BinaryWriter bw = new BinaryWriter(fs)) {
					bw.Seek(0, SeekOrigin.End);
					bw.Write(id);
					bw.Write(pos);
				}
			}
		}

		/// <summary>
		/// Usa pesquisa binária para percorrer um arquivo de indíces e retorna a posição do registro procurado no arquivo Contas.db.
		/// </summary>
		/// <param name="index">Indíce que está sendo procurado (ID)</param>
		/// <returns>Posição do registro no arquivo Conta.db.</returns>
		public long FindPosByIndex(ushort index) {
			using (FileStream fs = new FileStream(pathIndexDB, FileMode.Open, FileAccess.Read)) {
				using (BinaryReader br = new BinaryReader(fs)) {
					long len = br.BaseStream.Length, inf = 0, sup = len, meio;
					br.BaseStream.Seek(0, SeekOrigin.Begin);
					while (br.PeekChar() != -1 && inf <= sup) {
						meio = (inf + sup) / 2;

						if (meio % 10 <= 5) {
							meio -= meio % 10;
						} else {
							meio += meio % 10;
						}

						br.BaseStream.Position = meio;

						ushort id = br.ReadUInt16();
						if (id == index) {
							return br.ReadInt64();
						} else if (id > index) {
							sup = meio;
						} else {
							inf = meio;
						}
					}
				}
			}
			return -1;
		}

		public void CreateInvertedFiles() {
			List<ContaBancaria> list = ReadAll();
			List<string> cities = new List<string>();
			List<string> names = new List<string>();
			List<ushort> ids = new List<ushort>();

			for (int i = 0; i < list.Count; i++) {
				if (!cities.Contains(list[i].Cidade)) {
					cities.Add(list[i].Cidade);
				}
				if (!names.Contains(list[i].NomePessoa)) {
					names.Add(list[i].NomePessoa);
				}
			}

			cities.Sort();
			names.Sort();

			using (FileStream fs = new FileStream(pathInvertedCityDB, FileMode.Open, FileAccess.Write)) {
				using (BinaryWriter bw = new BinaryWriter(fs)) {
					for (int i = 0; i < cities.Count; i++) {
						ids = ReadAll(cities[i]);
						bw.Write(cities[i]);
						bw.Write(ids.Count);
						for (int j = 0; j < ids.Count; j++) {
							bw.Write(ids[j]);
						}
					}
				}
			}

			using (FileStream fs = new FileStream(pathInvertedNameDB, FileMode.Open, FileAccess.Write)) {
				using (BinaryWriter bw = new BinaryWriter(fs)) {
					for (int i = 0; i < names.Count; i++) {
						ids = ReadAll(names[i]);
						bw.Write(names[i]);
						bw.Write(ids.Count);
						for (int j = 0; j < ids.Count; j++) {
							bw.Write(ids[j]);
						}
					}
				}
			}
		}
	}
}
